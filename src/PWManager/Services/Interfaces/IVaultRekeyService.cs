using System.Threading.Tasks;
using PWManager.Domain.Model;
using PWManager.Enums;

namespace PWManager.Services.Interfaces;

/// <summary>
/// Re-encrypts the whole vault with a new master password.
/// </summary>
public interface IVaultRekeyService
{
    Task<RekeyResult> ChangeMasterPasswordAsync(string currentPassword, string newPassword);
}
