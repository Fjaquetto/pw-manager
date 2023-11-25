using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace PWManager.MAUI.Config
{
    public static class ConfigurationHelper
    {
        public static IConfiguration LoadConfiguration(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var jsonConfig = reader.ReadToEnd();
            var jsonDoc = JsonDocument.Parse(jsonConfig);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(jsonDoc.RootElement.EnumerateObject()
                    .Select(kvp => new KeyValuePair<string, string>(kvp.Name, kvp.Value.ToString())))
                .Build();

            return config;
        }
    }

}
