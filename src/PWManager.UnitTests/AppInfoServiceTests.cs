using PWManager.Services;

namespace PWManager.UnitTests;

public class AppInfoServiceTests
{
    [Theory]
    [InlineData("2.3.1", "2.3.1", null)]
    [InlineData("2.3.1+34819203c8f488692f908a5b20772ab725f82fa6", "2.3.1", "3481920")]
    [InlineData("2.3.1-alpha.0.7+34819203c8f488692f908a5b20772ab725f82fa6", "2.3.1-alpha.0.7", "3481920")]
    [InlineData("2.3.1+", "2.3.1", null)]
    [InlineData(null, "1.2.3", null)]
    [InlineData("", "1.2.3", null)]
    public void Parse_ExtractsVersionAndShortCommit(string? informationalVersion, string expectedVersion, string? expectedCommit)
    {
        var info = new AppInfoService(informationalVersion, new Version(1, 2, 3));

        Assert.Equal(expectedVersion, info.Version);
        Assert.Equal(expectedCommit, info.Commit);
    }

    [Fact]
    public void Parse_PadsAPartialAssemblyVersion()
    {
        var info = new AppInfoService(null, new Version(1, 2));

        Assert.Equal("1.2.0", info.Version);
    }

    [Fact]
    public void CurrentBuild_HasAVersion()
    {
        var info = new AppInfoService();

        Assert.False(string.IsNullOrWhiteSpace(info.Version));
    }
}
