using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application;
using PWManager.Application.DataContracts;
using PWManager.Application.Forms;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Forms.Config;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;

namespace PWManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.Configure<DatabaseConfig>(options => configuration.GetSection("DatabaseConfig").Bind(options));
            services.AddSingleton<DatabaseConfigurator>();
            services.AddSingleton<IPWDbContextFactory>(sp =>
            {
                return new PWDbContextFactory(sp.GetRequiredService<DatabaseConfigurator>().GetDatabaseFilePath());
            });
            services.AddScoped(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext());
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserApplication, UserApplication>();
            services.AddScoped<IUserEncryptorService, UserEncryptorService>();
            services.AddScoped<PWManagerKey>();
            services.AddScoped<PWManager>();
            services.AddScoped(sp => new Lazy<PWManager>(() => sp.GetRequiredService<PWManager>()));

            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
                context.Database.Migrate();
            }

            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(serviceProvider.GetRequiredService<PWManagerKey>());
        }
    }
}