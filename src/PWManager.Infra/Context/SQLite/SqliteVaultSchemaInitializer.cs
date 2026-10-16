using PWManager.Domain.DataContracts.InfraService;
using PWManager.Infra.Context.SQLite.DataContracts;

namespace PWManager.Infra.Context.SQLite;

public sealed class SqliteVaultSchemaInitializer(IPWDbContextFactory contextFactory) : IVaultSchemaInitializer
{
    public Task EnsureAsync()
    {
        using var context = contextFactory.CreateDbContext();
        context.Database.EnsureCreated();
        VaultSchemaInitializer.CreateVaultMetadataTableIfMissing(context);

        return Task.CompletedTask;
    }
}
