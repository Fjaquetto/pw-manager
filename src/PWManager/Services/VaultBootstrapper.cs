using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public sealed class VaultBootstrapper(
    IServiceScopeFactory scopeFactory,
    IUnlockService unlockService) : IVaultBootstrapper
{
    // Microsoft.Data.Sqlite executes "async" commands synchronously, so this has
    // to own a worker thread or the shell would block while the status resolves.
    public Task<VaultStatus> InitializeAsync() => Task.Run(async () =>
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
        context.Database.EnsureCreated();

        return await unlockService.GetStatusAsync();
    });
}
