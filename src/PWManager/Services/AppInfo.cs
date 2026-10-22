using System;
using System.Reflection;

namespace PWManager.Services;

public sealed record AppInfo(string Version, string? Commit)
{
    private const int CommitLength = 7;

    public static AppInfo FromAssembly(Assembly assembly) =>
        Parse(
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            assembly.GetName().Version);

    public static AppInfo Parse(string? informationalVersion, Version? assemblyVersion)
    {
        var fallback = assemblyVersion is null
            ? "unknown"
            : $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{Math.Max(assemblyVersion.Build, 0)}";

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return new AppInfo(fallback, null);
        }

        var parts = informationalVersion.Split('+', 2);
        var version = parts[0].Trim();

        if (version.Length == 0)
        {
            version = fallback;
        }

        var commit = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        if (commit.Length == 0)
        {
            return new AppInfo(version, null);
        }

        return new AppInfo(version, commit.Length <= CommitLength ? commit : commit[..CommitLength]);
    }
}
