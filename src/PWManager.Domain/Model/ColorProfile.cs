namespace PWManager.Domain.Model;

/// <summary>
/// A named, persisted appearance profile. The built-in "Default" profile is
/// implicit and never stored as a file.
/// </summary>
public sealed record ColorProfile
{
    public const int CurrentVersion = 1;

    public const string DefaultProfileName = "Default";

    public const int MaxNameLength = 40;

    public string Name { get; init; } = string.Empty;

    public int Version { get; init; } = CurrentVersion;

    public ColorScheme Colors { get; init; } = new();

    public static bool IsDefaultName(string? name) =>
        string.Equals(name?.Trim(), DefaultProfileName, StringComparison.OrdinalIgnoreCase);

    public static bool IsValidName(string? name)
    {
        var trimmed = name?.Trim();

        return !string.IsNullOrEmpty(trimmed)
            && trimmed.Length <= MaxNameLength
            && !IsDefaultName(trimmed)
            && trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }
}
