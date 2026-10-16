using System.Security.Cryptography;
using PWManager.Application.DataContracts;
using PWManager.Application.Enums;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;

namespace PWManager.Application;

public sealed class UnlockService(
    IVaultMetadataRepository metadataRepository,
    IUserRepository userRepository,
    IVaultKeyService keyService,
    IVaultDataMigrator dataMigrator,
    IVaultSession session,
    TimeProvider timeProvider) : IUnlockService
{
    public async Task<VaultStatus> GetStatusAsync()
    {
        if (await metadataRepository.AnyAsync())
        {
            return VaultStatus.Existing;
        }

        return await userRepository.AnyAsync() ? VaultStatus.Existing : VaultStatus.New;
    }

    public async Task<UnlockResult> UnlockAsync(string password)
    {
        try
        {
            var metadata = await metadataRepository.GetAsync();

            if (metadata is not null)
            {
                return UnlockExisting(metadata, password);
            }

            return await userRepository.AnyAsync()
                ? await MigrateLegacyVaultAsync(password)
                : await CreateVaultAsync(password);
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
        var key = keyService.DeriveKey(password, salt, metadata.KdfIterations);

        if (!keyService.VerifyKey(key, metadata.Verifier))
        {
            CryptographicOperations.ZeroMemory(key);
            return UnlockResult.InvalidPassword;
        }

        session.Unlock(key);
        return UnlockResult.Success;
    }

    private async Task<UnlockResult> CreateVaultAsync(string password)
    {
        var salt = keyService.CreateSalt();
        var key = keyService.DeriveKey(password, salt, VaultMetadata.DefaultIterations);
        var metadata = new VaultMetadata
        {
            Id = 1,
            KdfSalt = Convert.ToBase64String(salt),
            Verifier = keyService.CreateVerifier(key),
            CreatedAt = timeProvider.GetLocalNow().DateTime
        };

        await metadataRepository.AddAsync(metadata);

        session.Unlock(key);
        return UnlockResult.Success;
    }

    private async Task<UnlockResult> MigrateLegacyVaultAsync(string password)
    {
        var salt = keyService.CreateSalt();
        var key = keyService.DeriveKey(password, salt, VaultMetadata.DefaultIterations);
        var metadata = new VaultMetadata
        {
            Id = 1,
            KdfSalt = Convert.ToBase64String(salt),
            Verifier = keyService.CreateVerifier(key),
            CreatedAt = timeProvider.GetLocalNow().DateTime
        };

        var result = await dataMigrator.MigrateLegacyAsync(password, key, metadata);

        if (result == VaultMigrationResult.Success)
        {
            session.Unlock(key);
            return UnlockResult.Success;
        }

        CryptographicOperations.ZeroMemory(key);
        session.Lock();

        return result == VaultMigrationResult.InvalidPassword
            ? UnlockResult.InvalidPassword
            : UnlockResult.Unavailable;
    }
}
