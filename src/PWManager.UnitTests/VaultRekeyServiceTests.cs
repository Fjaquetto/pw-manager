using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using PWManager.Application;
using PWManager.Application.Enums;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;

namespace PWManager.UnitTests;

public class VaultRekeyServiceTests : IDisposable
{
    private const int TestIterations = 1000;

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pwmanager-rekey-" + Guid.NewGuid().ToString("N"));
    private readonly string _databasePath;
    private readonly VaultSession _session = new();
    private readonly VaultRekeyService _service;
    private byte[] _oldKey = Array.Empty<byte>();
    private string _oldSalt = string.Empty;

    public VaultRekeyServiceTests()
    {
        Directory.CreateDirectory(_directory);
        _databasePath = Path.Combine(_directory, "pwmanager.db");

        var factory = new PWDbContextFactory(_databasePath);
        using (var context = new PWDbContext(_databasePath))
        {
            context.Database.EnsureCreated();
            VaultSchemaInitializer.CreateVaultMetadataTableIfMissing(context);

            var salt = VaultCrypto.CreateSalt();
            _oldSalt = Convert.ToBase64String(salt);
            _oldKey = VaultCrypto.DeriveKey("current-password", salt, TestIterations);
            context.VaultMetadata.Add(new VaultMetadata
            {
                Id = 1,
                KdfSalt = _oldSalt,
                KdfIterations = TestIterations,
                Verifier = VaultCrypto.CreateVerifier(_oldKey)
            });
            context.User.Add(new UserEncryptorService(Unlocked(_oldKey)).EncryptUser(new User("GitHub", "alice", "secret")));
            context.SaveChanges();
        }

        _session.Unlock(_oldKey);

        _service = new VaultRekeyService(
            new VaultMetadataRepository(factory.CreateDbContext()),
            new VaultKeyService(),
            new VaultDataMigrator(factory),
            _session);
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
    public async Task Change_ReencryptsEverythingAndSwapsTheSessionKey()
    {
        var result = await _service.ChangeMasterPasswordAsync("current-password", "new-password");

        Assert.Equal(RekeyResult.Success, result);
        Assert.False(File.Exists(_databasePath + ".rekey.bak"));

        using var context = new PWDbContext(_databasePath);
        var metadata = context.VaultMetadata.Single();

        Assert.NotEqual(_oldSalt, metadata.KdfSalt);
        Assert.Equal(VaultMetadata.DefaultIterations, metadata.KdfIterations);
        Assert.True(VaultCrypto.VerifyKey(_session.GetKey(), metadata.Verifier));

        var entry = context.User.Single();

        Assert.Equal("GitHub", VaultCrypto.Decrypt(_session.GetKey(), entry.Site));
        Assert.Equal("alice", VaultCrypto.Decrypt(_session.GetKey(), entry.Login));
        Assert.Equal("secret", VaultCrypto.Decrypt(_session.GetKey(), entry.Password));
        Assert.ThrowsAny<CryptographicException>(() => VaultCrypto.Decrypt(_oldKey, entry.Site));
    }

    [Fact]
    public async Task Change_RejectsTheWrongCurrentPassword()
    {
        var result = await _service.ChangeMasterPasswordAsync("wrong-password", "new-password");

        Assert.Equal(RekeyResult.InvalidCurrentPassword, result);

        using var context = new PWDbContext(_databasePath);
        var entry = context.User.Single();

        Assert.Equal("GitHub", VaultCrypto.Decrypt(_oldKey, entry.Site));
        Assert.True(VaultCrypto.VerifyKey(_oldKey, context.VaultMetadata.Single().Verifier));
    }

    [Fact]
    public async Task Change_WithoutMetadata_IsUnavailable()
    {
        var directory = Path.Combine(_directory, "empty");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "empty.db");
        var factory = new PWDbContextFactory(path);

        using (var context = new PWDbContext(path))
        {
            context.Database.EnsureCreated();
            VaultSchemaInitializer.CreateVaultMetadataTableIfMissing(context);
        }

        var service = new VaultRekeyService(
            new VaultMetadataRepository(factory.CreateDbContext()),
            new VaultKeyService(),
            new VaultDataMigrator(factory),
            new VaultSession());

        Assert.Equal(RekeyResult.Unavailable, await service.ChangeMasterPasswordAsync("a", "b"));
    }

    [Fact]
    public async Task Change_WithCorruptData_RollsBackAndKeepsTheOldState()
    {
        using (var context = new PWDbContext(_databasePath))
        {
            context.User.Single().Login = "corrupt-not-v2";
            context.SaveChanges();
        }

        var result = await _service.ChangeMasterPasswordAsync("current-password", "new-password");

        Assert.Equal(RekeyResult.Unavailable, result);

        using (var context = new PWDbContext(_databasePath))
        {
            var entry = context.User.Single();

            Assert.Equal("corrupt-not-v2", entry.Login);
            Assert.Equal("GitHub", VaultCrypto.Decrypt(_oldKey, entry.Site));
            Assert.True(VaultCrypto.VerifyKey(_oldKey, context.VaultMetadata.Single().Verifier));
        }
    }

    private static VaultSession Unlocked(byte[] key)
    {
        var session = new VaultSession();
        session.Unlock(key);

        return session;
    }
}
