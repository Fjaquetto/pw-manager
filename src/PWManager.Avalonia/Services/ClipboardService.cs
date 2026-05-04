using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace PWManager.Avalonia.Services;

public class ClipboardService
{
    public async Task SetTextAsync(string text)
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var clipboard = desktop.MainWindow?.Clipboard as IClipboard;
            if (clipboard != null)
                await clipboard.SetTextAsync(text);
        }
    }
}
