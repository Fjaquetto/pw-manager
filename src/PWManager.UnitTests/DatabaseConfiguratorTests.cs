using Microsoft.Extensions.Options;
using PWManager.Infra.Config;

namespace PWManager.UnitTests;

public class DatabaseConfiguratorTests
{
    [Fact]
    public void ResolveDataFolder_UsesRoamingAppDataOnWindows()
    {
        Assert.Equal(
            Environment.SpecialFolder.ApplicationData,
            DatabaseConfigurator.ResolveDataFolder(isWindows: true));
    }

    [Fact]
    public void ResolveDataFolder_UsesLocalAppDataOnOtherPlatforms()
    {
        Assert.Equal(
            Environment.SpecialFolder.LocalApplicationData,
            DatabaseConfigurator.ResolveDataFolder(isWindows: false));
    }

    [Fact]
    public void GetDatabaseFilePath_UsesDefaultFolderAndFileNames()
    {
        var root = CreateTemporaryRoot();
        try
        {
            Assert.False(Directory.Exists(root));
            var configurator = CreateConfigurator(new DatabaseConfig());

            var path = configurator.GetDatabaseFilePath(root);

            Assert.Equal(Path.Combine(root, "PWManager", "pwmanager.db"), path);
            Assert.True(Directory.Exists(Path.Combine(root, "PWManager")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GetDatabaseFilePath_CombinesConfiguredFolderAndFile()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var configurator = CreateConfigurator(new DatabaseConfig { AppFolder = "CustomApp", DbFile = "custom.db" });

            var path = configurator.GetDatabaseFilePath(root);

            Assert.Equal(Path.Combine(root, "CustomApp", "custom.db"), path);
            Assert.True(Directory.Exists(Path.Combine(root, "CustomApp")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PlatformDataFolder_IsAvailableOnCurrentOperatingSystem()
    {
        var dataFolder = Environment.GetFolderPath(
            DatabaseConfigurator.ResolveDataFolder(OperatingSystem.IsWindows()));

        Assert.False(string.IsNullOrWhiteSpace(dataFolder));
    }

    private static DatabaseConfigurator CreateConfigurator(DatabaseConfig config)
    {
        return new DatabaseConfigurator(Options.Create(config));
    }

    private static string CreateTemporaryRoot()
    {
        return Path.Combine(Path.GetTempPath(), "pwmanager-configurator-" + Guid.NewGuid().ToString("N"));
    }
}
