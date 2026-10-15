using System.Threading.Tasks;
using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

public interface IVaultBootstrapper
{
    Task<VaultStatus> InitializeAsync();
}
