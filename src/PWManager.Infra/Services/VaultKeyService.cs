using PWManager.Domain.DataContracts.InfraService;

namespace PWManager.Infra.Services;

public sealed class VaultKeyService : IVaultKeyService
{
    public byte[] CreateSalt() => VaultCrypto.CreateSalt();

    public byte[] DeriveKey(string password, byte[] salt, int iterations) =>
        VaultCrypto.DeriveKey(password, salt, iterations);

    public string CreateVerifier(byte[] key) => VaultCrypto.CreateVerifier(key);

    public bool VerifyKey(byte[] key, string verifier) => VaultCrypto.VerifyKey(key, verifier);
}
