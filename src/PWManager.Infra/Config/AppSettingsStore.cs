using System.Text.Json;
using PWManager.Domain.Model;

namespace PWManager.Infra.Config;

/// <summary>
/// Persists the user preferences as JSON next to the vault. Preferences are
/// not secrets and are deliberately kept out of the database.
/// </summary>
public sealed class AppSettingsStore
{
    public const string FileName = "settings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _directory;

    public AppSettingsStore(string directory)
    {
        _directory = directory;
    }

    public string FilePath => Path.Combine(_directory, FileName);

    public AppSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            return new AppSettings();
        }

        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOptions);

            return (settings ?? new AppSettings()).Normalized();
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(_directory);
        var normalized = settings.Normalized();
        var temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(normalized, JsonOptions));
        File.Move(temporary, FilePath, overwrite: true);
    }
}
