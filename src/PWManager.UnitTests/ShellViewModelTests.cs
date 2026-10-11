using Moq;
using PWManager.Domain.Model;
using PWManager.Enums;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;

namespace PWManager.UnitTests;

public class ShellViewModelTests
{
    [Fact]
    public void StartsLockedWithoutCreatingTheVault()
    {
        var created = 0;
        var shell = CreateShell(() => { created++; return new TestVault().ViewModel; });

        Assert.True(shell.ShowUnlock);
        Assert.False(shell.IsUnlocked);
        Assert.False(shell.IsOpening);
        Assert.False(shell.ShowProgress);
        Assert.Null(shell.Vault);
        Assert.Equal(0, created);
    }

    [Fact]
    public async Task SuccessfulUnlock_LoadsTheVaultBeforeRevealingIt()
    {
        var encrypted = new TaskCompletionSource<IEnumerable<User>>();
        var vault = new TestVault();
        vault.Application.Setup(a => a.GetAllUsersAsync()).Returns(encrypted.Task);
        var shell = CreateShell(() => vault.ViewModel, UnlockResult.Success);

        await shell.Unlock.UnlockCommand.ExecuteAsync(null);

        Assert.True(shell.IsOpening);
        Assert.True(shell.ShowProgress);
        Assert.False(shell.IsUnlocked);
        Assert.True(shell.ShowUnlock);

        encrypted.SetResult(Array.Empty<User>());

        await shell.OpeningTask!;

        Assert.False(shell.IsOpening);
        Assert.True(shell.IsUnlocked);
        Assert.False(shell.ShowUnlock);
        Assert.NotNull(shell.Vault);
    }

    [Fact]
    public async Task SuccessfulUnlock_CreatesTheVaultExactlyOnce()
    {
        var created = 0;
        var shell = CreateShell(() => { created++; return new TestVault().ViewModel; }, UnlockResult.Success);

        await shell.Unlock.UnlockCommand.ExecuteAsync(null);
        await shell.OpeningTask!;
        await shell.OpenVaultAsync();

        Assert.Equal(1, created);
    }

    [Theory]
    [InlineData(UnlockResult.InvalidPassword)]
    [InlineData(UnlockResult.Unavailable)]
    public async Task FailedUnlock_NeverCreatesTheVault(UnlockResult result)
    {
        var created = 0;
        var shell = CreateShell(() => { created++; return new TestVault().ViewModel; }, result);

        await shell.Unlock.UnlockCommand.ExecuteAsync(null);

        Assert.True(shell.ShowUnlock);
        Assert.False(shell.IsUnlocked);
        Assert.Null(shell.Vault);
        Assert.Null(shell.OpeningTask);
        Assert.Equal(0, created);
        Assert.True(shell.Unlock.HasError);
    }

    [Fact]
    public async Task ProgressIndicator_CoversPasswordValidationAndVaultLoading()
    {
        var pendingUnlock = new TaskCompletionSource<UnlockResult>();
        var unlockService = new Mock<IUnlockService>();
        unlockService.Setup(s => s.UnlockAsync(It.IsAny<string>())).Returns(pendingUnlock.Task);
        var encrypted = new TaskCompletionSource<IEnumerable<User>>();
        var vault = new TestVault();
        vault.Application.Setup(a => a.GetAllUsersAsync()).Returns(encrypted.Task);
        var shell = new ShellViewModel(new UnlockViewModel(unlockService.Object), () => vault.ViewModel)
        {
            Unlock = { Password = "master" }
        };

        var unlocking = shell.Unlock.UnlockCommand.ExecuteAsync(null);

        Assert.True(shell.ShowProgress);

        pendingUnlock.SetResult(UnlockResult.Success);

        await unlocking;

        Assert.True(shell.ShowProgress);
        Assert.True(shell.IsOpening);
        Assert.False(shell.IsUnlocked);

        encrypted.SetResult(Array.Empty<User>());
        await shell.OpeningTask!;

        Assert.False(shell.ShowProgress);
        Assert.True(shell.IsUnlocked);
    }

    private static ShellViewModel CreateShell(Func<MainViewModel> factory, UnlockResult result = UnlockResult.Success)
    {
        var unlockService = new Mock<IUnlockService>();
        unlockService.Setup(s => s.UnlockAsync(It.IsAny<string>())).ReturnsAsync(result);
        var unlock = new UnlockViewModel(unlockService.Object)
        {
            Password = "master"
        };

        return new ShellViewModel(unlock, factory);
    }
}
