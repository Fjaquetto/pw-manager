using PWManager.Domain.Model;

namespace PWManager.Domain.DataContracts.Repository;

public interface IVaultMetadataRepository
{
    Task<bool> AnyAsync();

    Task<VaultMetadata?> GetAsync();

    Task AddAsync(VaultMetadata metadata);
}
