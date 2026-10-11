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
        return Path.Combine(GetApplicationFolder(), databaseConfigOptions.Value.DbFile);
    }

    public string GetDatabaseFilePath(string dataFolder)
    {
        return Path.Combine(GetApplicationFolder(dataFolder), databaseConfigOptions.Value.DbFile);
    }

    public string GetApplicationFolder()
    {
        var dataFolder = Environment.GetFolderPath(ResolveDataFolder(OperatingSystem.IsWindows()));
        return GetApplicationFolder(dataFolder);
    }

    public string GetApplicationFolder(string dataFolder)
    {
        var applicationFolder = Path.Combine(dataFolder, databaseConfigOptions.Value.AppFolder);
        Directory.CreateDirectory(applicationFolder);
        return applicationFolder;
    }
}
