using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;

namespace PWManager.Application;

public sealed class VaultBootstrapper(
    IVaultSchemaInitializer schemaInitializer,
    IUnlockService unlockService) : IVaultBootstrapper
{
    // Microsoft.Data.Sqlite executes "async" commands synchronously, so this has
    // to own a worker thread or the shell would block while the status resolves.
    public Task<VaultStatus> InitializeAsync() => Task.Run(async () =>
    {
        await schemaInitializer.EnsureAsync();

        return await unlockService.GetStatusAsync();
    });
}
