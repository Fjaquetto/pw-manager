using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;
using PWManager.Views;

namespace PWManager.Services;

public class NavigationService : INavigationService
{
    public void ShowMain()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainViewModel = App.Services.GetRequiredService<MainViewModel>();
                var mainView = new MainView
                {
                    DataContext = mainViewModel
                };
                desktop.MainWindow = mainView;
                mainView.Show();
                var unlockWindows = desktop.Windows.OfType<UnlockView>().ToList();

                foreach (var unlockWindow in unlockWindows)
                {
                    unlockWindow.Close();
                }
            }
        });
    }
}
