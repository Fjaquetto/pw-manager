namespace PWManager.Domain.DataContracts.InfraService;

public interface IVaultSession
{
    bool IsUnlocked { get; }

    void Unlock(byte[] key);

    void Lock();

    byte[] GetKey();
}
