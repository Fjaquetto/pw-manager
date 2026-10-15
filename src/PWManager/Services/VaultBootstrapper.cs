using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

/// <summary>
/// Prepares the vault storage during startup, away from the UI thread. The
/// SQLite provider executes "async" commands synchronously on the calling
/// thread, so the whole pass must run on a worker thread for the shell to stay
/// responsive while the status is resolved.
/// </summary>
public sealed class VaultBootstrapper(
    IServiceScopeFactory scopeFactory,
    IUnlockService unlockService) : IVaultBootstrapper
{
    public Task<VaultStatus> InitializeAsync() => Task.Run(async () =>
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
        context.Database.EnsureCreated();

        return await unlockService.GetStatusAsync();
    });
}
