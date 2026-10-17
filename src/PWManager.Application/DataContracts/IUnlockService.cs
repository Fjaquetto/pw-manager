using PWManager.Application.Enums;
using PWManager.Domain.Model;

namespace PWManager.Application.DataContracts;

public interface IUnlockService
{
    Task<VaultStatus> GetStatusAsync();

    Task<UnlockResult> UnlockAsync(string password);
}
