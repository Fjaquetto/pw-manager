using System.Threading.Tasks;
using PWManager.Domain.Model;
using PWManager.Enums;

namespace PWManager.Services.Interfaces;

public interface IUnlockService
{
    Task<VaultStatus> GetStatusAsync();

    Task<UnlockResult> UnlockAsync(string password);
}
