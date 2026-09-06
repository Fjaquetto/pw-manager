using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Infra.Config;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;
using PWManager.Services;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;
using PWManager.Views;
using AvaloniaApp = Avalonia.Application;

namespace PWManager;

public partial class App : AvaloniaApp
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.Configure<DatabaseConfig>(options => configuration.GetSection("DatabaseConfig").Bind(options));
        serviceCollection.AddSingleton<DatabaseConfigurator>();
        serviceCollection.AddSingleton<IPWDbContextFactory>(provider =>
            new PWDbContextFactory(provider.GetRequiredService<DatabaseConfigurator>().GetDatabaseFilePath()));
        serviceCollection.AddScoped(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext());
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IUserApplication, PWManager.Application.UserApplication>();
        serviceCollection.AddScoped<IUserEncryptorService, UserEncryptorService>();
        serviceCollection.AddTransient<INavigationService, NavigationService>();
        serviceCollection.AddTransient<IClipboardService, ClipboardService>();
        serviceCollection.AddTransient<IUnlockService, UnlockService>();
        serviceCollection.AddTransient<IVaultEntryService, VaultEntryService>();
        serviceCollection.AddSingleton(TimeProvider.System);
        serviceCollection.AddTransient<UnlockViewModel>();
        serviceCollection.AddTransient<MainViewModel>();
        Services = serviceCollection.BuildServiceProvider();
        using (var scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
            context.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var unlockViewModel = Services.GetRequiredService<UnlockViewModel>();
            var unlockView = new UnlockView
            {
                DataContext = unlockViewModel
            };
            desktop.MainWindow = unlockView;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
