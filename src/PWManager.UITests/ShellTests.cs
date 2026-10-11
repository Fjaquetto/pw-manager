using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PWManager.Enums;
using PWManager.UITests.Fakes;
using PWManager.ViewModels;
using PWManager.Views;

namespace PWManager.UITests;

public class ShellTests
{
    private static T Find<T>(Window window, string name)
        where T : Control => window.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private static int Created;

    private static ShellWindow Open(UnlockStub service)
    {
        var shell = new ShellViewModel(new UnlockViewModel(service), () =>
        {
            Created++;
            return MemoryVault.Sample(3).CreateViewModel();
        });
        var window = new ShellWindow { DataContext = shell };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return window;
    }

    private static void PumpUntil(Func<bool> condition, int attempts = 100)
    {
        for (var i = 0; i < attempts && !condition(); i++)
        {
            Thread.Sleep(5);
            Dispatcher.UIThread.RunJobs();
        }

        Assert.True(condition(), "Condition was not reached while pumping the dispatcher.");
    }

    [AvaloniaFact]
    public void Shell_StartsLocked_FocusesPasswordAndNeverCreatesTheVault()
    {
        Created = 0;
        var window = Open(new UnlockStub());

        try
        {
            var password = Find<TextBox>(window, "PasswordBox");
            var shell = (ShellViewModel)window.DataContext!;

            Assert.True(password.IsFocused);
            Assert.Equal('•', password.PasswordChar);
            Assert.True(Find<ContentControl>(window, "UnlockHost").IsEffectivelyVisible);
            Assert.False(Find<ContentControl>(window, "VaultHost").IsEffectivelyVisible);
            Assert.Null(shell.Vault);
            Assert.Equal(0, Created);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Shell_ShowsProgressWhileUnlocking_ThenShowsTheErrorAndStaysLocked()
    {
        Created = 0;
        var service = new UnlockStub { Pending = new() };
        var window = Open(service);

        try
        {
            var shell = (ShellViewModel)window.DataContext!;

            window.KeyTextInput("incorrect-example");
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, service.Calls);
            Assert.True(shell.ShowProgress);
            Assert.False(shell.Unlock.UnlockCommand.CanExecute(null));
            Assert.False(Find<ContentControl>(window, "VaultHost").IsEffectivelyVisible);

            service.Pending.SetResult(UnlockResult.InvalidPassword);
            PumpUntil(() => shell.Unlock.HasError);

            Assert.False(shell.ShowProgress);
            Assert.False(shell.IsUnlocked);
            Assert.True(Find<ContentControl>(window, "UnlockHost").IsEffectivelyVisible);
            Assert.Equal(0, Created);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Shell_SuccessfulUnlock_LoadsRevealsResizesAndFocusesTheVault()
    {
        Created = 0;
        var window = Open(new UnlockStub { Result = UnlockResult.Success });

        try
        {
            var shell = (ShellViewModel)window.DataContext!;

            window.KeyTextInput("master");
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            PumpUntil(() => shell.IsUnlocked);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, Created);
            Assert.NotNull(shell.Vault);
            Assert.Equal(3, shell.Vault!.Entries.Count);
            Assert.False(Find<ContentControl>(window, "UnlockHost").IsEffectivelyVisible);
            Assert.True(Find<ContentControl>(window, "VaultHost").IsEffectivelyVisible);
            Assert.True(window.GetVisualDescendants().OfType<VaultPanel>().Single().IsEffectivelyVisible);
            Assert.True(window.Width >= 1000);
            Assert.True(window.Height >= 600);
            Assert.True(Find<TextBox>(window, "SearchBox").IsFocused);

            window.KeyPress(Key.F, RawInputModifiers.Control, PhysicalKey.F, null);
            window.KeyRelease(Key.F, RawInputModifiers.Control, PhysicalKey.F, null);
            Dispatcher.UIThread.RunJobs();

            Assert.True(Find<TextBox>(window, "SearchBox").IsFocused);
        }
        finally
        {
            window.Close();
        }
    }
}
