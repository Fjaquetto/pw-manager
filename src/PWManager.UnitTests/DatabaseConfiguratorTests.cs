using Microsoft.Extensions.Options;
using PWManager.Infra.Config;

namespace PWManager.UnitTests;

public class DatabaseConfiguratorTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolveDataFolder_AlwaysUsesLocalAppData(bool isWindows)
    {
        Assert.Equal(
            Environment.SpecialFolder.LocalApplicationData,
            DatabaseConfigurator.ResolveDataFolder(isWindows));
    }

    [Fact]
    public void DataFolderMigration_MovesTheVaultAndThemesFromRoamingToLocal()
    {
        var roaming = Path.Combine(Path.GetTempPath(), "pwmanager-roaming-" + Guid.NewGuid().ToString("N"));
        var local = Path.Combine(Path.GetTempPath(), "pwmanager-local-" + Guid.NewGuid().ToString("N"));
        var source = Path.Combine(roaming, "PWManager");
        Directory.CreateDirectory(Path.Combine(source, "themes"));
        File.WriteAllText(Path.Combine(source, "pwmanager.db"), "db");
        File.WriteAllText(Path.Combine(source, "pwmanager.db-wal"), "wal");
        File.WriteAllText(Path.Combine(source, "themes", "ocean.json"), "{}");

        try
        {
            DataFolderMigration.MoveIfNeeded(roaming, local, "PWManager");

            var target = Path.Combine(local, "PWManager");

            Assert.True(File.Exists(Path.Combine(target, "pwmanager.db")));
            Assert.True(File.Exists(Path.Combine(target, "pwmanager.db-wal")));
            Assert.True(File.Exists(Path.Combine(target, "themes", "ocean.json")));
            Assert.False(File.Exists(Path.Combine(source, "pwmanager.db")));

            // Running again is a no-op and never overwrites the local vault.
            File.WriteAllText(Path.Combine(target, "pwmanager.db"), "local-db");
            DataFolderMigration.MoveIfNeeded(roaming, local, "PWManager");

            Assert.Equal("local-db", File.ReadAllText(Path.Combine(target, "pwmanager.db")));
        }
        finally
        {
            Directory.Delete(roaming, recursive: true);
            Directory.Delete(local, recursive: true);
        }
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
