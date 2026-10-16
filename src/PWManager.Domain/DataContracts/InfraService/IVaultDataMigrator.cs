using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts.InfraService;

public interface IVaultDataMigrator
{
    Task<VaultMigrationResult> MigrateLegacyAsync(string legacyPassword, byte[] newKey, VaultMetadata newMetadata);

    Task<VaultMigrationResult> ReencryptAsync(byte[] currentKey, byte[] newKey, VaultMetadata updatedMetadata);
}
