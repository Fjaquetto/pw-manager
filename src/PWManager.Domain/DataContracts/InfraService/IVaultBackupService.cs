namespace PWManager.Domain.DataContracts.InfraService;

/// <summary>
/// Backs up and restores the vault as a single encrypted database file.
/// </summary>
public interface IVaultBackupService
{
    Task BackupAsync(string targetPath);

    /// <summary>
    /// Replaces the current vault with the backup and returns the path of the
    /// safety copy taken from the previous vault.
    /// </summary>
    Task<string> RestoreAsync(string sourcePath);
}
