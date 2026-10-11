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

    /// <summary>
    /// True for the built-in names: the Default palette and the shipped presets.
    /// </summary>
    public static bool IsReservedName(string? name) => IsDefaultName(name) || ColorSchemePresets.IsPresetName(name);

    /// <summary>
    /// A name that can be used as a file name for a user profile.
    /// </summary>
    public static bool IsWellFormedName(string? name)
    {
        var trimmed = name?.Trim();

        return !string.IsNullOrEmpty(trimmed)
            && trimmed.Length <= MaxNameLength
            && trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    public static bool IsValidName(string? name) => IsWellFormedName(name) && !IsReservedName(name);
}
