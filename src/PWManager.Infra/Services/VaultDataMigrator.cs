using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;

namespace PWManager.Infra.Services;

public sealed class VaultDataMigrator(IPWDbContextFactory contextFactory) : IVaultDataMigrator
{
    public async Task<VaultMigrationResult> MigrateLegacyAsync(string legacyPassword, byte[] newKey, VaultMetadata newMetadata)
    {
        using var context = contextFactory.CreateDbContext();
        var backupPath = $"{contextFactory.DatabasePath}.migrating.bak";
        context.Database.ExecuteSqlRaw("VACUUM INTO {0}", backupPath);

        using var transaction = context.Database.BeginTransaction();

        try
        {
            var entries = await context.User.ToListAsync();

            foreach (var entry in entries)
            {
                entry.Site = VaultCrypto.Encrypt(newKey, LegacyVaultCrypto.Decrypt(entry.Site, legacyPassword));
                entry.Login = VaultCrypto.Encrypt(newKey, LegacyVaultCrypto.Decrypt(entry.Login, legacyPassword));
                entry.Password = VaultCrypto.Encrypt(newKey, LegacyVaultCrypto.Decrypt(entry.Password, legacyPassword));
            }

            await context.SaveChangesAsync();

            foreach (var entry in entries)
            {
                VaultCrypto.Decrypt(newKey, entry.Site);
                VaultCrypto.Decrypt(newKey, entry.Login);
                VaultCrypto.Decrypt(newKey, entry.Password);
            }

            context.VaultMetadata.Add(newMetadata);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (CryptographicException)
        {
            await transaction.RollbackAsync();
            DeleteBackup(backupPath);
            return VaultMigrationResult.InvalidPassword;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return VaultMigrationResult.Failed;
        }

        DeleteBackup(backupPath);
        return VaultMigrationResult.Success;
    }

    public async Task<VaultMigrationResult> ReencryptAsync(byte[] currentKey, byte[] newKey, VaultMetadata updatedMetadata)
    {
        using var context = contextFactory.CreateDbContext();
        var metadata = await context.VaultMetadata.FirstOrDefaultAsync();

        if (metadata is null)
        {
            return VaultMigrationResult.Failed;
        }

        var backupPath = $"{contextFactory.DatabasePath}.rekey.bak";
        context.Database.ExecuteSqlRaw("VACUUM INTO {0}", backupPath);

        using var transaction = context.Database.BeginTransaction();

        try
        {
            var entries = await context.User.ToListAsync();

            foreach (var entry in entries)
            {
                entry.Site = VaultCrypto.Encrypt(newKey, VaultCrypto.Decrypt(currentKey, entry.Site));
                entry.Login = VaultCrypto.Encrypt(newKey, VaultCrypto.Decrypt(currentKey, entry.Login));
                entry.Password = VaultCrypto.Encrypt(newKey, VaultCrypto.Decrypt(currentKey, entry.Password));
            }

            metadata.Kdf = updatedMetadata.Kdf;
            metadata.KdfIterations = updatedMetadata.KdfIterations;
            metadata.KdfSalt = updatedMetadata.KdfSalt;
            metadata.Verifier = updatedMetadata.Verifier;
            metadata.FormatVersion = updatedMetadata.FormatVersion;

            await context.SaveChangesAsync();

            foreach (var entry in entries)
            {
                VaultCrypto.Decrypt(newKey, entry.Site);
                VaultCrypto.Decrypt(newKey, entry.Login);
                VaultCrypto.Decrypt(newKey, entry.Password);
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return VaultMigrationResult.Failed;
        }

        DeleteBackup(backupPath);
        return VaultMigrationResult.Success;
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
