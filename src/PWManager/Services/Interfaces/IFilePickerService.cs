using System.Threading.Tasks;

namespace PWManager.Services.Interfaces;

/// <summary>
/// Native file dialogs for backup and restore.
/// </summary>
public interface IFilePickerService
{
    Task<string?> PickSaveFileAsync(string title, string suggestedFileName);

    Task<string?> PickOpenFileAsync(string title);
}
