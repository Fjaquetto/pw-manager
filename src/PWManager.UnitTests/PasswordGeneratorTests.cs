using PWManager.Avalonia.ViewModels;
using System.Linq;
using Xunit;

namespace PWManager.UnitTests;

public class PasswordGeneratorTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(20)]
    [InlineData(48)]
    public void GeneratePassword_ReturnsCorrectLength(int length)
    {
        var result = PasswordGeneratorViewModel.GeneratePassword(length, true, true, true, true);

        Assert.Equal(length, result.Length);
    }

    [Fact]
    public void GeneratePassword_OnlyUppercase_ContainsOnlyUppercaseLetters()
    {
        var result = PasswordGeneratorViewModel.GeneratePassword(50, upper: true, lower: false, numbers: false, symbols: false);

        Assert.True(result.All(char.IsUpper),
            $"Expected only uppercase letters but got: {result}");
    }

    [Fact]
    public void GeneratePassword_OnlyNumbers_ContainsOnlyDigits()
    {
        var result = PasswordGeneratorViewModel.GeneratePassword(50, upper: false, lower: false, numbers: true, symbols: false);

        Assert.True(result.All(char.IsDigit),
            $"Expected only digits but got: {result}");
    }

    [Fact]
    public void GeneratePassword_NoOptionsSelected_FallsBackToLowercase()
    {
        var result = PasswordGeneratorViewModel.GeneratePassword(30, upper: false, lower: false, numbers: false, symbols: false);

        Assert.Equal(30, result.Length);
        Assert.True(result.All(char.IsLower),
            $"Expected fallback to lowercase but got: {result}");
    }

    [Fact]
    public void GeneratePassword_AllOptions_ContainsMixedCharacters()
    {
        var result = PasswordGeneratorViewModel.GeneratePassword(100, upper: true, lower: true, numbers: true, symbols: true);

        Assert.Equal(100, result.Length);
        Assert.Contains(result, char.IsUpper);
        Assert.Contains(result, char.IsLower);
        Assert.Contains(result, char.IsDigit);
        Assert.Contains(result, c => !char.IsLetterOrDigit(c));
    }

    [Fact]
    public void GeneratePassword_CalledTwice_ReturnsDifferentPasswords()
    {
        var first  = PasswordGeneratorViewModel.GeneratePassword(32, true, true, true, true);
        var second = PasswordGeneratorViewModel.GeneratePassword(32, true, true, true, true);

        Assert.NotEqual(first, second);
    }
}
