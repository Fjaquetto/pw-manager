using Moq;
using PWManager.Services.Interfaces;

namespace PWManager.UnitTests;

internal static class TestAppInfo
{
    public const string Version = "2.3.1";

    public const string Commit = "3481920";

    public static IAppInfoService Create() =>
        Mock.Of<IAppInfoService>(info => info.Version == Version && info.Commit == Commit);
}
