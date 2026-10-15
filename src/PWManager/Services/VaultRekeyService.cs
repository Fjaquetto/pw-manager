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

public sealed class VaultRekeyService(
    IServiceScopeFactory scopeFactory,
    IVaultSession session,
    IPWDbContextFactory dbFactory) : IVaultRekeyService
{
    public async Task<RekeyResult> ChangeMasterPasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
            var metadata = await context.VaultMetadata.FirstOrDefaultAsync();

            if (metadata is null)
            {
                return RekeyResult.Unavailable;
            }

            var currentSalt = Convert.FromBase64String(metadata.KdfSalt);
            var currentKey = VaultCrypto.DeriveKey(currentPassword, currentSalt, metadata.KdfIterations);

            if (!VaultCrypto.VerifyKey(currentKey, metadata.Verifier))
            {
                CryptographicOperations.ZeroMemory(currentKey);
                return RekeyResult.InvalidCurrentPassword;
            }

            var newSalt = VaultCrypto.CreateSalt();
            var newKey = VaultCrypto.DeriveKey(newPassword, newSalt, VaultMetadata.DefaultIterations);
            var backupPath = $"{dbFactory.DatabasePath}.rekey.bak";

            context.Database.ExecuteSqlRaw("VACUUM INTO {0}", backupPath);

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var entries = await context.User.ToListAsync();

                    foreach (var entry in entries)
                    {
                        entry.Site = VaultCrypto.Encrypt(newKey, VaultCrypto.Decrypt(currentKey, entry.Site));
                        entry.Login = VaultCrypto.Encrypt(newKey, VaultCrypto.Decrypt(currentKey, entry.Login));
                        entry.Password = VaultCrypto.Encrypt(newKey, VaultCrypto.Decrypt(currentKey, entry.Password));
                    }

                    metadata.Kdf = VaultMetadata.Pbkdf2Sha256;
                    metadata.KdfIterations = VaultMetadata.DefaultIterations;
                    metadata.KdfSalt = Convert.ToBase64String(newSalt);
                    metadata.Verifier = VaultCrypto.CreateVerifier(newKey);

                    await context.SaveChangesAsync();

                    foreach (var entry in entries)
                    {
                        VaultCrypto.Decrypt(newKey, entry.Site);
                        VaultCrypto.Decrypt(newKey, entry.Login);
                        VaultCrypto.Decrypt(newKey, entry.Password);
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    CryptographicOperations.ZeroMemory(newKey);
                    CryptographicOperations.ZeroMemory(currentKey);
                    return RekeyResult.Unavailable;
                }
            }

            DeleteBackup(backupPath);
            CryptographicOperations.ZeroMemory(currentKey);
            session.Unlock(newKey);
            return RekeyResult.Success;
        }
        catch (Exception)
        {
            return RekeyResult.Unavailable;
        }
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
