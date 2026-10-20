using System;
using System.Reflection;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public sealed class AppInfoService : IAppInfoService
{
    private const int CommitLength = 7;

    public AppInfoService()
        : this(
            typeof(AppInfoService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            typeof(AppInfoService).Assembly.GetName().Version)
    {
    }

    internal AppInfoService(string? informationalVersion, Version? assemblyVersion)
    {
        (Version, Commit) = Parse(informationalVersion, assemblyVersion);
    }

    public string Version { get; }

    public string? Commit { get; }

    internal static (string Version, string? Commit) Parse(string? informationalVersion, Version? assemblyVersion)
    {
        var fallback = assemblyVersion is null
            ? "unknown"
            : $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{Math.Max(assemblyVersion.Build, 0)}";

        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return (fallback, null);
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
            return (version, null);
        }

        return (version, commit.Length <= CommitLength ? commit : commit[..CommitLength]);
    }
}
