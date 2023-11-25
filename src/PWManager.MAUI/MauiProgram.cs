using Microsoft.Extensions.Configuration;
using PWManager.Domain.DataContracts;
using PWManager.Infra.Config;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;
using PWManager.MAUI.Config;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace PWManager.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //var configuration = ConfigurationHelper.LoadConfiguration($"{Assembly.GetExecutingAssembly().GetName().Name}.appsettings.json");

            //builder.Services.Configure<DatabaseConfig>(options => configuration.GetSection("DatabaseConfig").Bind(options));
            //builder.Services.AddSingleton<DatabaseConfigurator>();
            //builder.Services.AddSingleton<IPWDbContextFactory>(sp =>
            //{
            //    return new PWDbContextFactory(sp.GetRequiredService<DatabaseConfigurator>().GetDatabaseFilePath());
            //});
            //builder.Services.AddScoped(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext());
            //builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //builder.Services.AddScoped<IUserEncryptorService, UserEncryptorService>();

            return builder.Build();
        }
    }
}