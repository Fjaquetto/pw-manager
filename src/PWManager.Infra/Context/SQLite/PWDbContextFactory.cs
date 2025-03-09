using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Infra.Config;
using PWManager.Infra.Context.SQLite.DataContracts;


namespace PWManager.Infra.Context.SQLite
{
    public class PWDbContextFactory : IPWDbContextFactory, IDesignTimeDbContextFactory<PWDbContext>
    {
        private readonly string _dbPath;

        public PWDbContextFactory(string dbPath)
        {
            _dbPath = dbPath;
        }

        // Constructor for design-time factory (used by migrations)
        public PWDbContextFactory()
        {
            var services = new ServiceCollection();
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
                
            services.Configure<DatabaseConfig>(options => configuration.GetSection("DatabaseConfig").Bind(options));
            services.AddSingleton<DatabaseConfigurator>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            var databaseConfigurator = serviceProvider.GetRequiredService<DatabaseConfigurator>();
            
            _dbPath = databaseConfigurator.GetDatabaseFilePath();
        }

        public PWDbContext CreateDbContext()
        {
            return new PWDbContext(_dbPath);
        }

        // Implementation for IDesignTimeDbContextFactory
        public PWDbContext CreateDbContext(string[] args)
        {
            return CreateDbContext();
        }
    }
}
