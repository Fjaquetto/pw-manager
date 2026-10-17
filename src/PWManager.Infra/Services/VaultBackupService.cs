using Microsoft.Data.Sqlite;
using PWManager.Domain.DataContracts.InfraService;

namespace PWManager.Infra.Services;

// VACUUM INTO produces a consistent snapshot without the WAL side files, and
// no plaintext ever leaves the vault.
public sealed class VaultBackupService(string databasePath) : IVaultBackupService
{
    public string DatabasePath => databasePath;

    public async Task BackupAsync(string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var fullTarget = Path.GetFullPath(targetPath);

        if (string.Equals(fullTarget, Path.GetFullPath(databasePath), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Choose a different file for the backup.");
        }

        var directory = Path.GetDirectoryName(fullTarget);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(fullTarget))
        {
            File.Delete(fullTarget);
        }

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "VACUUM INTO $target;";
        command.Parameters.AddWithValue("$target", fullTarget);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<bool> IsValidBackupAsync(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            return false;
        }

        try
        {
            await using var connection = new SqliteConnection($"Data Source={sourcePath};Mode=ReadOnly");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'User';";

            return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
        }
        catch (SqliteException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<string> RestoreAsync(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        var fullSource = Path.GetFullPath(sourcePath);

        if (string.Equals(fullSource, Path.GetFullPath(databasePath), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The selected file is the current vault.");
        }

        if (!await IsValidBackupAsync(fullSource))
        {
            throw new InvalidDataException("The selected file is not a PWManager backup.");
        }

        var safetyCopy = $"{databasePath}.before-restore-{DateTime.Now:yyyyMMddHHmmss}.bak";
        await BackupAsync(safetyCopy);

        SqliteConnection.ClearAllPools();
        File.Copy(fullSource, databasePath, overwrite: true);

        foreach (var sideFile in new[] { databasePath + "-wal", databasePath + "-shm" })
        {
            if (File.Exists(sideFile))
            {
                File.Delete(sideFile);
            }
        }

        return safetyCopy;
    }
}
