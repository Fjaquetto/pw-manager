using System.Threading.Tasks;

namespace PWManager.Services.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickSaveFileAsync(string title, string suggestedFileName);

    Task<string?> PickOpenFileAsync(string title);
}
