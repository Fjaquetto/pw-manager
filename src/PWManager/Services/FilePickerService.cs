using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public sealed class FilePickerService(Func<Window?> windowAccessor) : IFilePickerService
{
    private static readonly FilePickerFileType BackupFileType = new("PWManager backup")
    {
        Patterns = ["*.db"]
    };

    public async Task<string?> PickSaveFileAsync(string title, string suggestedFileName)
    {
        var window = windowAccessor();

        if (window is null)
        {
            return null;
        }

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "db",
            FileTypeChoices = [BackupFileType]
        });

        return file?.TryGetLocalPath();
    }

    public async Task<string?> PickOpenFileAsync(string title)
    {
        var window = windowAccessor();

        if (window is null)
        {
            return null;
        }

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [BackupFileType]
        });

        return files.FirstOrDefault()?.TryGetLocalPath();
    }
}
