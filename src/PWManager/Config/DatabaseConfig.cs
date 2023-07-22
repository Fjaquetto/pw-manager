using Microsoft.Extensions.Options;

namespace PWManager.Forms.Config
{
    public class DatabaseConfig
    {
        public string AppFolder { get; set; }
        public string DbFile { get; set; }
    }

    public class DatabaseConfigurator
    {
        private readonly DatabaseConfig _databaseConfig;

        public DatabaseConfigurator(IOptions<DatabaseConfig> databaseConfigOptions)
        {
            _databaseConfig = databaseConfigOptions.Value;
        }

        public string GetDatabaseFilePath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, _databaseConfig.AppFolder);

            Directory.CreateDirectory(appFolder);

            string dbFilePath = Path.Combine(appFolder, _databaseConfig.DbFile);
            return dbFilePath;
        }
    }
}
