using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Enums;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;
using PWManager.Infra.Services;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

/// <summary>
/// Opens (or creates) the vault. Vaults written before the v2 format are
/// detected and migrated to AES-GCM with a single PBKDF2-SHA256 derivation
/// when the correct password is provided.
/// </summary>
public sealed class UnlockService(
    IServiceScopeFactory scopeFactory,
    IVaultSession session,
    IPWDbContextFactory dbFactory,
    TimeProvider timeProvider) : IUnlockService
{
    public async Task<VaultStatus> GetStatusAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
        VaultSchemaInitializer.EnsureUpToDate(context);

        if (await context.VaultMetadata.AnyAsync())
        {
            return VaultStatus.Existing;
        }

        return await context.User.AnyAsync() ? VaultStatus.Existing : VaultStatus.New;
    }

    public async Task<UnlockResult> UnlockAsync(string password)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
            VaultSchemaInitializer.EnsureUpToDate(context);

            var metadata = await context.VaultMetadata.FirstOrDefaultAsync();

            if (metadata is not null)
            {
                return UnlockExisting(metadata, password);
            }

            if (!await context.User.AnyAsync())
            {
                return CreateVault(context, password);
            }

            return await MigrateLegacyVaultAsync(context, password);
        }
        catch (CryptographicException)
        {
            session.Lock();
            return UnlockResult.InvalidPassword;
        }
        catch (Exception)
        {
            session.Lock();
            return UnlockResult.Unavailable;
        }
    }

    private UnlockResult UnlockExisting(VaultMetadata metadata, string password)
    {
        var salt = Convert.FromBase64String(metadata.KdfSalt);
        var key = VaultCrypto.DeriveKey(password, salt, metadata.KdfIterations);

        if (!VaultCrypto.VerifyKey(key, metadata.Verifier))
        {
            CryptographicOperations.ZeroMemory(key);
            return UnlockResult.InvalidPassword;
        }

        session.Unlock(key);
        return UnlockResult.Success;
    }

    private UnlockResult CreateVault(PWDbContext context, string password)
    {
        var salt = VaultCrypto.CreateSalt();
        var key = VaultCrypto.DeriveKey(password, salt, VaultMetadata.DefaultIterations);

        context.VaultMetadata.Add(new VaultMetadata
        {
            Id = 1,
            KdfSalt = Convert.ToBase64String(salt),
            Verifier = VaultCrypto.CreateVerifier(key),
            CreatedAt = timeProvider.GetLocalNow().DateTime
        });
        context.SaveChanges();

        session.Unlock(key);
        return UnlockResult.Success;
    }

    private async Task<UnlockResult> MigrateLegacyVaultAsync(PWDbContext context, string password)
    {
        var backupPath = $"{dbFactory.DatabasePath}.migrating.bak";
        context.Database.ExecuteSqlRaw("VACUUM INTO {0}", backupPath);

        var salt = VaultCrypto.CreateSalt();
        var key = VaultCrypto.DeriveKey(password, salt, VaultMetadata.DefaultIterations);

        using var transaction = context.Database.BeginTransaction();

        try
        {
            var entries = await context.User.ToListAsync();

            foreach (var entry in entries)
            {
                entry.Site = VaultCrypto.Encrypt(key, LegacyVaultCrypto.Decrypt(entry.Site, password));
                entry.Login = VaultCrypto.Encrypt(key, LegacyVaultCrypto.Decrypt(entry.Login, password));
                entry.Password = VaultCrypto.Encrypt(key, LegacyVaultCrypto.Decrypt(entry.Password, password));
            }

            await context.SaveChangesAsync();

            foreach (var entry in entries)
            {
                VaultCrypto.Decrypt(key, entry.Site);
                VaultCrypto.Decrypt(key, entry.Login);
                VaultCrypto.Decrypt(key, entry.Password);
            }

            context.VaultMetadata.Add(new VaultMetadata
            {
                Id = 1,
                KdfSalt = Convert.ToBase64String(salt),
                Verifier = VaultCrypto.CreateVerifier(key),
                CreatedAt = timeProvider.GetLocalNow().DateTime
            });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (CryptographicException)
        {
            await transaction.RollbackAsync();
            CryptographicOperations.ZeroMemory(key);
            DeleteBackup(backupPath);
            return UnlockResult.InvalidPassword;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            CryptographicOperations.ZeroMemory(key);
            return UnlockResult.Unavailable;
        }

        DeleteBackup(backupPath);
        session.Unlock(key);
        return UnlockResult.Success;
    }

    private static void DeleteBackup(string backupPath)
    {
        try
        {
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
        catch (IOException)
        {
        }
    }
}
