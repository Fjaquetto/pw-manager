using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace PWManager.Infra.Config;

public class DatabaseConfigurator(IOptions<DatabaseConfig> databaseConfigOptions)
{
    public string GetDatabaseFilePath()
    {
        var applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var databaseConfig = databaseConfigOptions.Value;
        var applicationFolder = Path.Combine(applicationDataFolder, databaseConfig.AppFolder);
        Directory.CreateDirectory(applicationFolder);
        return Path.Combine(applicationFolder, databaseConfig.DbFile);
    }
}
