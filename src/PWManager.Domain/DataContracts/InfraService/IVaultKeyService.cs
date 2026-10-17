namespace PWManager.Domain.DataContracts.InfraService;

public interface IVaultKeyService
{
    byte[] CreateSalt();

    byte[] DeriveKey(string password, byte[] salt, int iterations);

    string CreateVerifier(byte[] key);

    bool VerifyKey(byte[] key, string verifier);
}
