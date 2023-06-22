using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.Forms;
using PWManager.Domain.DataContracts;
using PWManager.Infra.Context;
using PWManager.Infra.Context.DataContracts;
using PWManager.Infra.Repository;


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
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "PWManager");
            Directory.CreateDirectory(appFolder);
            string dbFilePath = Path.Combine(appFolder, "pwmanager.db");

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IPWDbContextFactory>(new PWDbContextFactory(dbFilePath))
                .AddScoped<PWDbContext>(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext())
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
                context.Database.Migrate();
            }

            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new FWManagerKey(serviceProvider));
        }
    }
}