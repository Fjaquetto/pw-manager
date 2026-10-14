using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public class ClipboardService : IClipboardService
{
    public async Task<bool> SetTextAsync(string text)
    {
        if (ResolveClipboard() is not { } clipboard)
        {
            return false;
        }

        await clipboard.SetTextAsync(text);

        return true;
    }

    public async Task<string?> TryGetTextAsync()
    {
        if (ResolveClipboard() is not { } clipboard)
        {
            return null;
        }

        return await clipboard.TryGetTextAsync();
    }

    public async Task ClearAsync()
    {
        if (ResolveClipboard() is not { } clipboard)
        {
            return;
        }

        await clipboard.ClearAsync();
    }

    private static IClipboard? ResolveClipboard()
    {
        return global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow?.Clipboard
            : null;
    }
}
