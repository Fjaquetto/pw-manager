namespace PWManager.Domain.Model;

/// <summary>
/// Crypto metadata of the vault. Stored inside the database so it travels
/// with backups; never contains secrets (salt and verifier only).
/// </summary>
public class VaultMetadata
{
    public const int CurrentFormatVersion = 2;

    public const string Pbkdf2Sha256 = "PBKDF2-SHA256";

    public const int DefaultIterations = 600_000;

    public int Id { get; set; } = 1;

    public int FormatVersion { get; set; } = CurrentFormatVersion;

    public string Kdf { get; set; } = Pbkdf2Sha256;

    public int KdfIterations { get; set; } = DefaultIterations;

    public string KdfSalt { get; set; } = string.Empty;

    public string Verifier { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
