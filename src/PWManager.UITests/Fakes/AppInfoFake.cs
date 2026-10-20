using PWManager.Services.Interfaces;

namespace PWManager.UITests.Fakes;

internal sealed class AppInfoFake : IAppInfoService
{
    public string Version { get; init; } = "2.3.1";

    public string? Commit { get; init; } = "3481920";
}
