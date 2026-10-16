using System.Security.Cryptography;
using PWManager.Application.DataContracts;
using PWManager.Application.Enums;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;

namespace PWManager.Application;

public sealed class VaultRekeyService(
    IVaultMetadataRepository metadataRepository,
    IVaultKeyService keyService,
    IVaultDataMigrator dataMigrator,
    IVaultSession session) : IVaultRekeyService
{
    public async Task<RekeyResult> ChangeMasterPasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var metadata = await metadataRepository.GetAsync();

            if (metadata is null)
            {
                return RekeyResult.Unavailable;
            }

            var currentSalt = Convert.FromBase64String(metadata.KdfSalt);
            var currentKey = keyService.DeriveKey(currentPassword, currentSalt, metadata.KdfIterations);

            if (!keyService.VerifyKey(currentKey, metadata.Verifier))
            {
                CryptographicOperations.ZeroMemory(currentKey);
                return RekeyResult.InvalidCurrentPassword;
            }

            var newSalt = keyService.CreateSalt();
            var newKey = keyService.DeriveKey(newPassword, newSalt, VaultMetadata.DefaultIterations);
            var updatedMetadata = new VaultMetadata
            {
                Id = metadata.Id,
                FormatVersion = VaultMetadata.CurrentFormatVersion,
                Kdf = VaultMetadata.Pbkdf2Sha256,
                KdfIterations = VaultMetadata.DefaultIterations,
                KdfSalt = Convert.ToBase64String(newSalt),
                Verifier = keyService.CreateVerifier(newKey),
                CreatedAt = metadata.CreatedAt
            };

            var result = await dataMigrator.ReencryptAsync(currentKey, newKey, updatedMetadata);

            if (result != VaultMigrationResult.Success)
            {
                CryptographicOperations.ZeroMemory(currentKey);
                CryptographicOperations.ZeroMemory(newKey);
                return RekeyResult.Unavailable;
            }

            CryptographicOperations.ZeroMemory(currentKey);
            session.Unlock(newKey);
            return RekeyResult.Success;
        }
        catch (Exception)
        {
            return RekeyResult.Unavailable;
        }
    }
}
