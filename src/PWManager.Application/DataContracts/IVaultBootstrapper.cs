using PWManager.Domain.Model;

namespace PWManager.Application.DataContracts;

public interface IVaultBootstrapper
{
    Task<VaultStatus> InitializeAsync();
}
