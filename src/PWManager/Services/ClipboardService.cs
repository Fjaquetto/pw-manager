using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public class ClipboardService : IClipboardService
{
    public async Task<bool> SetTextAsync(string text)
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow?.Clipboard is not { } clipboard)
        {
            return false;
        }

        await clipboard.SetTextAsync(text);

        return true;
    }
}
