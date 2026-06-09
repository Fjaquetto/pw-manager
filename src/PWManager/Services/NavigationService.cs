using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PWManager.ViewModels;
using PWManager.Views;
using System.Linq;

namespace PWManager.Services;

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
