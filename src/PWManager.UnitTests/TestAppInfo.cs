using PWManager.Services;

namespace PWManager.UnitTests;

internal static class TestAppInfo
{
    public static AppInfo Create() => new("2.3.1", "3481920");
}
