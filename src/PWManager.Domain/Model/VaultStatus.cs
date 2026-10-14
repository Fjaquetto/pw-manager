namespace PWManager.Domain.Model;

/// <summary>
/// Whether the vault already has a master password (existing) or still has to
/// be created on first use (new).
/// </summary>
public enum VaultStatus
{
    New,
    Existing
}
