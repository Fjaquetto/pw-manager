using System.Threading.Tasks;
using PWManager.Enums;

namespace PWManager.Services.Interfaces;

public interface IUnlockService
{
    Task<UnlockResult> UnlockAsync(string password);
}
