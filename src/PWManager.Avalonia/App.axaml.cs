using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.DataContracts;
using PWManager.Avalonia.Services;
using PWManager.Avalonia.ViewModels;
using PWManager.Avalonia.Views;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Infra.Config;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Context.SQLite.DataContracts;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;
using System;
using System.IO;
using AvaloniaApp = Avalonia.Application;

namespace PWManager.Avalonia;

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
        serviceCollection.AddSingleton<IPWDbContextFactory>(sp =>
            new PWDbContextFactory(sp.GetRequiredService<DatabaseConfigurator>().GetDatabaseFilePath()));
        serviceCollection.AddScoped(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext());
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IUserApplication, PWManager.Application.UserApplication>();
        serviceCollection.AddScoped<IUserEncryptorService, UserEncryptorService>();
        serviceCollection.AddTransient<NavigationService>();
        serviceCollection.AddTransient<ClipboardService>();
        serviceCollection.AddTransient<UnlockViewModel>();
        serviceCollection.AddTransient<MainViewModel>();

        Services = serviceCollection.BuildServiceProvider();

        using (var scope = ((Microsoft.Extensions.DependencyInjection.IServiceScopeFactory)Services
            .GetService(typeof(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory))!).CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PWDbContext>();
            context.Database.EnsureCreated();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var unlockVm = (UnlockViewModel)Services.GetService(typeof(UnlockViewModel))!;
            var unlockView = new UnlockView { DataContext = unlockVm };
            desktop.MainWindow = unlockView;
        }

        base.OnFrameworkInitializationCompleted();
    }
}