namespace PWManager.Services.Interfaces;

public interface IAppInfoService
{
    string Version { get; }

    string? Commit { get; }
}
