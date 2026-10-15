using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
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

        MoveRoamingDataIfNeeded(configuration);

        Services = BuildServices(configuration);

        Services.GetRequiredService<IThemeService>()
            .Apply(Services.GetRequiredService<ColorThemeStore>().LoadActiveScheme());

        var shellViewModel = Services.GetRequiredService<ShellViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new ShellWindow
            {
                DataContext = shellViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();

        // Vault preparation is deliberately started after the window has been
        // assigned: it must never delay the first frame.
        _ = PrepareVaultAsync(shellViewModel);
    }

    private IServiceProvider BuildServices(IConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.Configure<DatabaseConfig>(options => configuration.GetSection("DatabaseConfig").Bind(options));
        serviceCollection.AddSingleton<DatabaseConfigurator>();
        serviceCollection.AddSingleton<IPWDbContextFactory>(provider =>
            new PWDbContextFactory(provider.GetRequiredService<DatabaseConfigurator>().GetDatabaseFilePath()));
        serviceCollection.AddScoped(sp => sp.GetRequiredService<IPWDbContextFactory>().CreateDbContext());
        serviceCollection.AddSingleton<IVaultBootstrapper, VaultBootstrapper>();
        serviceCollection.AddSingleton<ColorThemeStore>(provider => new ColorThemeStore(
            Path.Combine(
                provider.GetRequiredService<DatabaseConfigurator>().GetApplicationFolder(),
                ColorThemeStore.ThemesFolderName)));
        serviceCollection.AddSingleton<IThemeService, ThemeService>();
        serviceCollection.AddSingleton<IVaultSession, VaultSession>();
        serviceCollection.AddSingleton<AppSettingsStore>(provider =>
            new AppSettingsStore(provider.GetRequiredService<DatabaseConfigurator>().GetApplicationFolder()));
        serviceCollection.AddSingleton<IAppSettingsService, AppSettingsService>();
        serviceCollection.AddSingleton<IVaultBackupService>(provider =>
            new VaultBackupService(provider.GetRequiredService<IPWDbContextFactory>().DatabasePath));
        serviceCollection.AddTransient<IVaultRekeyService, VaultRekeyService>();
        serviceCollection.AddSingleton<IFilePickerService>(_ => new FilePickerService(() =>
            ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null));
        serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IUserApplication, PWManager.Application.UserApplication>();
        serviceCollection.AddScoped<IUserEncryptorService, UserEncryptorService>();
        serviceCollection.AddTransient<IClipboardService, ClipboardService>();
        serviceCollection.AddTransient<IUnlockService, UnlockService>();
        serviceCollection.AddTransient<IVaultEntryService, VaultEntryService>();
        serviceCollection.AddSingleton(TimeProvider.System);
        serviceCollection.AddTransient<UnlockViewModel>();
        serviceCollection.AddTransient<MainViewModel>();
        serviceCollection.AddTransient<ShellViewModel>();
        serviceCollection.AddSingleton<Func<MainViewModel>>(provider => provider.GetRequiredService<MainViewModel>);

        return serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Resolves the vault status on a worker thread and hands it to the unlock
    /// screen on the UI thread. Failures fall back to "vault exists" so the
    /// unlock attempt can surface the error instead of application startup.
    /// </summary>
    private static async Task PrepareVaultAsync(ShellViewModel shell)
    {
        VaultStatus status;

        try
        {
            status = await Services.GetRequiredService<IVaultBootstrapper>().InitializeAsync();
        }
        catch (Exception exception)
        {
            Trace.TraceError($"Vault bootstrap failed: {exception}");
            status = VaultStatus.Existing;
        }

        await Dispatcher.UIThread.InvokeAsync(() => shell.Unlock.Configure(status));
    }

    private static void MoveRoamingDataIfNeeded(IConfiguration configuration)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        DataFolderMigration.MoveIfNeeded(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            configuration["DatabaseConfig:AppFolder"] ?? "PWManager");
    }
}
