using System.Threading.Tasks;

namespace PWManager.Services.Interfaces;

public interface IClipboardService
{
    Task<bool> SetTextAsync(string text);
}
