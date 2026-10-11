using Microsoft.Data.Sqlite;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Services;

namespace PWManager.UnitTests;

public class VaultBackupServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pwmanager-backup-" + Guid.NewGuid().ToString("N"));
    private readonly string _databasePath;
    private readonly VaultSession _session = new();

    public VaultBackupServiceTests()
    {
        Directory.CreateDirectory(_directory);
        _databasePath = Path.Combine(_directory, "pwmanager.db");
        _session.Unlock(VaultCrypto.DeriveKey("password", VaultCrypto.CreateSalt(), 1000));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task Backup_ProducesASingleConsistentFile()
    {
        AddEntry("GitHub");
        var target = Path.Combine(_directory, "backup.db");
        var service = new VaultBackupService(_databasePath);

        await service.BackupAsync(target);

        Assert.True(File.Exists(target));
        Assert.False(File.Exists(target + "-wal"));
        Assert.False(File.Exists(target + "-shm"));
        Assert.True(await VaultBackupService.IsValidBackupAsync(target));

        await using var connection = new SqliteConnection($"Data Source={target};Mode=ReadOnly");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM User;";

        Assert.Equal(1L, Convert.ToInt64(await command.ExecuteScalarAsync()));
    }

    [Fact]
    public async Task Restore_ReplacesTheVaultAndKeepsASafetyCopy()
    {
        AddEntry("GitHub");
        var backup = Path.Combine(_directory, "backup.db");
        var service = new VaultBackupService(_databasePath);
        await service.BackupAsync(backup);

        AddEntry("GitLab");
        Assert.Equal(2, CountEntries());

        var safetyCopy = await service.RestoreAsync(backup);

        Assert.Equal(1, CountEntries());
        Assert.True(File.Exists(safetyCopy));
        Assert.Equal(2, CountEntries(safetyCopy));
        Assert.False(File.Exists(_databasePath + "-wal"));
    }

    [Fact]
    public async Task Backup_RefusesToOverwriteTheLiveDatabase()
    {
        AddEntry("GitHub");
        var service = new VaultBackupService(_databasePath);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.BackupAsync(_databasePath));
    }

    [Fact]
    public async Task Restore_RejectsFilesThatAreNotVaults()
    {
        var foreign = Path.Combine(_directory, "foreign.db");
        await File.WriteAllTextAsync(foreign, "this is not a database");
        var service = new VaultBackupService(_databasePath);

        Assert.False(await VaultBackupService.IsValidBackupAsync(foreign));
        await Assert.ThrowsAsync<InvalidDataException>(() => service.RestoreAsync(foreign));
    }

    private void AddEntry(string site)
    {
        using var context = new PWDbContext(_databasePath);
        context.Database.EnsureCreated();
        VaultSchemaInitializer.EnsureUpToDate(context);
        context.User.Add(new UserEncryptorService(_session).EncryptUser(new User(site, "login", "secret")));
        context.SaveChanges();
    }

    private int CountEntries(string? path = null)
    {
        using var context = new PWDbContext(path ?? _databasePath);
        return context.User.Count();
    }
}
