using PWManager.Infra.Context.SQLite;

namespace PWManager.Infra.Context.SQLite.DataContracts
{
    public interface IPWDbContextFactory
    {
        string DatabasePath { get; }

        PWDbContext CreateDbContext();
    }
}
