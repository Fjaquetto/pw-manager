using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.Forms;
using PWManager.Domain.DataContracts;
using PWManager.Infra.Context;
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
            var serviceProvider = new ServiceCollection()
                .AddDbContext<PWDbContext>(options => options.UseSqlite($"Data Source={System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\pwmanager.db"))}"))
                .AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .BuildServiceProvider();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new FWManagerKey(serviceProvider));
        }
    }
}