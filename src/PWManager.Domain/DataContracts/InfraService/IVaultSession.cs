namespace PWManager.Domain.DataContracts.InfraService;

/// <summary>
/// Holds the derived vault key for the unlocked session and zeroes it when the
/// vault is locked.
/// </summary>
public interface IVaultSession
{
    bool IsUnlocked { get; }

    void Unlock(byte[] key);

    void Lock();

    byte[] GetKey();
}
