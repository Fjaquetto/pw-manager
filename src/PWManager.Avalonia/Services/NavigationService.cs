using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PWManager.Avalonia.ViewModels;
using PWManager.Avalonia.Views;
using System.Linq;

namespace PWManager.Avalonia.Services;

public class NavigationService
{
    public void ShowMain()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainVm = App.Services.GetRequiredService<MainViewModel>();
                var mainView = new MainView { DataContext = mainVm };
                desktop.MainWindow = mainView;
                mainView.Show();

                var toClose = desktop.Windows.OfType<UnlockView>().ToList();
                foreach (var w in toClose)
                    w.Close();
            }
        });
    }
}
