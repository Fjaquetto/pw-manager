using PWManager.Application.Enums;

namespace PWManager.Application.DataContracts;

public interface IVaultRekeyService
{
    Task<RekeyResult> ChangeMasterPasswordAsync(string currentPassword, string newPassword);
}
