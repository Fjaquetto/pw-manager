using System.Threading.Tasks;
using PWManager.Domain.Model;
using PWManager.Enums;

namespace PWManager.Services.Interfaces;

public interface IVaultRekeyService
{
    Task<RekeyResult> ChangeMasterPasswordAsync(string currentPassword, string newPassword);
}
