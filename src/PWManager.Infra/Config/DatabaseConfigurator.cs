using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace PWManager.Infra.Config;

public class DatabaseConfigurator(IOptions<DatabaseConfig> databaseConfigOptions)
{
    public static Environment.SpecialFolder ResolveDataFolder(bool isWindows)
    {
        return isWindows
            ? Environment.SpecialFolder.ApplicationData
            : Environment.SpecialFolder.LocalApplicationData;
    }

    public string GetDatabaseFilePath()
    {
        var dataFolder = Environment.GetFolderPath(ResolveDataFolder(OperatingSystem.IsWindows()));
        return GetDatabaseFilePath(dataFolder);
    }

    public string GetDatabaseFilePath(string dataFolder)
    {
        var databaseConfig = databaseConfigOptions.Value;
        var applicationFolder = Path.Combine(dataFolder, databaseConfig.AppFolder);
        Directory.CreateDirectory(applicationFolder);
        return Path.Combine(applicationFolder, databaseConfig.DbFile);
    }
}
