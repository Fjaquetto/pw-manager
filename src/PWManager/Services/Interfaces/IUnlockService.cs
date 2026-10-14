using System.Threading.Tasks;
using PWManager.Domain.Model;
using PWManager.Enums;

namespace PWManager.Services.Interfaces;

public interface IUnlockService
{
    /// <summary>
    /// Whether the vault is new (no master password yet) or already exists.
    /// </summary>
    Task<VaultStatus> GetStatusAsync();

    Task<UnlockResult> UnlockAsync(string password);
}
