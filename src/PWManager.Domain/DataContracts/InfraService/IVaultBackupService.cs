namespace PWManager.Domain.DataContracts.InfraService;

public interface IVaultBackupService
{
    Task BackupAsync(string targetPath);

    // The caller must lock the session afterwards, so the vault can be unlocked
    // again with the backup's master password.
    Task<string> RestoreAsync(string sourcePath);
}
