using System.Threading.Tasks;
using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

/// <summary>
/// One-time startup preparation of the local vault storage. It runs off the
/// caller's thread because the SQLite provider has no true asynchronous I/O:
/// the shell must stay responsive while the vault status is resolved.
/// </summary>
public interface IVaultBootstrapper
{
    /// <summary>
    /// Ensures the database file and schema exist, then reports whether the
    /// user already has a vault.
    /// </summary>
    Task<VaultStatus> InitializeAsync();
}
