using Microsoft.EntityFrameworkCore;
using Moq;
using PWManager.Application;
using PWManager.Application.DataContracts;
using PWManager.Application.Enums;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Infra.Config;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;

namespace PWManager.UnitTests;

public class PersistenceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pwmanager-test-" + Guid.NewGuid().ToString("N"));
    private readonly string _path;

    public PersistenceTests()
    {
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "vault.db");
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public async Task TemporarySqlite_CreateEditReopenDelete_PreservesEncryptionAndDates()
    {
        var session = CreateUnlockedSession("Only-a-test-master-password");

        Guid id;
        DateTime created;

        using (var context = CreateContext())
        {
            var repository = new UserRepository(context);
            var vaultService = new VaultEntryService(repository, new UserEncryptorService(session), TimeProvider.System);
            using var viewModel = CreateViewModel(vaultService);

            await viewModel.LoadEntriesAsync();
            viewModel.NewEntryCommand.Execute(null);
            viewModel.Editor.Site = "Test account";
            viewModel.Editor.Login = "alex@example.com";
            viewModel.Editor.Password = "  keep these spaces  ";

            await viewModel.SaveEntryCommand.ExecuteAsync(null);

            Assert.False(viewModel.Notification.IsError);
            id = viewModel.SelectedEntry!.Id;
            created = viewModel.SelectedEntry.CreationDate;
            var encrypted = await repository.GetUserByIdAsync(id);

            Assert.True(VaultCrypto.IsV2(encrypted.Site));
            Assert.NotEqual("Test account", encrypted.Site);
            Assert.NotEqual("alex@example.com", encrypted.Login);
            Assert.NotEqual("  keep these spaces  ", encrypted.Password);
            viewModel.EditEntryCommand.Execute(null);
            viewModel.Editor.Login = "updated@example.com";

            await viewModel.SaveEntryCommand.ExecuteAsync(null);

            Assert.Equal(created, viewModel.SelectedEntry!.CreationDate);
        }

        using (var context = CreateContext())
        {
            var repository = new UserRepository(context);
            var vaultService = new VaultEntryService(repository, new UserEncryptorService(session), TimeProvider.System);
            using var viewModel = CreateViewModel(vaultService);

            await viewModel.LoadEntriesAsync();
            var restored = Assert.Single(viewModel.Entries);

            Assert.Equal(id, restored.Id);
            Assert.Equal(created, restored.CreationDate);
            Assert.Equal("updated@example.com", restored.Login);
            Assert.Equal("  keep these spaces  ", restored.Password);
            viewModel.SelectedEntry = restored;
            viewModel.RequestDeleteCommand.Execute(null);

            await viewModel.ConfirmDeleteCommand.ExecuteAsync(null);

            Assert.Empty(await repository.GetAllAsync());
        }
    }

    [Fact]
    public async Task UnlockService_EmptyVault_CreatesV2MetadataAndUnlocks()
    {
        var session = new VaultSession();
        var service = CreateUnlockService(session);
        using (CreateContext())
        {
        }

        Assert.Equal(VaultStatus.New, await service.GetStatusAsync());
        Assert.Equal(UnlockResult.Success, await service.UnlockAsync("first-example-password"));

        Assert.True(session.IsUnlocked);
        Assert.Equal(VaultStatus.Existing, await service.GetStatusAsync());

        using var context = CreateContext();
        var metadata = Assert.Single(context.VaultMetadata.ToList());

        Assert.Equal(VaultMetadata.CurrentFormatVersion, metadata.FormatVersion);
        Assert.Equal(VaultMetadata.Pbkdf2Sha256, metadata.Kdf);
        Assert.Equal(VaultMetadata.DefaultIterations, metadata.KdfIterations);
        Assert.True(VaultCrypto.VerifyKey(session.GetKey(), metadata.Verifier));
    }

    [Fact]
    public async Task UnlockService_ExistingV2Vault_RequiresTheCorrectPassword()
    {
        using (CreateContext())
        {
        }

        var creator = new VaultSession();
        var createService = CreateUnlockService(creator);
        Assert.Equal(UnlockResult.Success, await createService.UnlockAsync("correct-example"));

        var session = new VaultSession();
        var service = CreateUnlockService(session);

        Assert.Equal(VaultStatus.Existing, await service.GetStatusAsync());
        Assert.Equal(UnlockResult.InvalidPassword, await service.UnlockAsync("wrong-example"));
        Assert.False(session.IsUnlocked);
        Assert.Equal(UnlockResult.Success, await service.UnlockAsync("correct-example"));
        Assert.True(session.IsUnlocked);
    }

    [Fact]
    public async Task UnlockService_MigratesLegacyVault_AndIsIdempotent()
    {
        var legacyEntry = CreateLegacyEntry("correct-example");
        var originalSiteCipher = legacyEntry.Site;

        using (var context = CreateContext())
        {
            context.User.Add(legacyEntry);
            await context.SaveChangesAsync();
        }

        var session = new VaultSession();
        var service = CreateUnlockService(session);

        Assert.Equal(VaultStatus.Existing, await service.GetStatusAsync());
        Assert.Equal(UnlockResult.InvalidPassword, await service.UnlockAsync("wrong-example"));
        Assert.False(session.IsUnlocked);
        Assert.False(File.Exists(_path + ".migrating.bak"));

        using (var context = CreateContext())
        {
            var untouched = Assert.Single(context.User.ToList());
            Assert.Equal(originalSiteCipher, untouched.Site);
            Assert.Empty(context.VaultMetadata.ToList());
        }

        Assert.Equal(UnlockResult.Success, await service.UnlockAsync("correct-example"));

        Assert.True(session.IsUnlocked);
        Assert.False(File.Exists(_path + ".migrating.bak"));

        using (var context = CreateContext())
        {
            var migrated = Assert.Single(context.User.ToList());

            Assert.True(VaultCrypto.IsV2(migrated.Site));
            Assert.Equal("GitHub", VaultCrypto.Decrypt(session.GetKey(), migrated.Site));
            Assert.Equal("alice", VaultCrypto.Decrypt(session.GetKey(), migrated.Login));
            Assert.Equal("secret", VaultCrypto.Decrypt(session.GetKey(), migrated.Password));
            Assert.Single(context.VaultMetadata.ToList());
        }

        // A second unlock uses the v2 metadata and does not re-migrate.
        var secondSession = new VaultSession();
        var secondService = CreateUnlockService(secondSession);

        Assert.Equal(UnlockResult.Success, await secondService.UnlockAsync("correct-example"));

        using var finalContext = CreateContext();
        var stable = Assert.Single(finalContext.User.ToList());

        Assert.Equal("GitHub", VaultCrypto.Decrypt(secondSession.GetKey(), stable.Site));
    }

    [Fact]
    public async Task UnlockService_WrongPasswordOnV2Vault_LeavesDataReadable()
    {
        using (CreateContext())
        {
        }

        var creator = new VaultSession();
        var createService = CreateUnlockService(creator);
        Assert.Equal(UnlockResult.Success, await createService.UnlockAsync("correct-example"));

        var user = new User("Site", "login", "secret");
        using (var context = CreateContext())
        {
            context.User.Add(new UserEncryptorService(creator).EncryptUser(user));
            await context.SaveChangesAsync();
        }

        var session = new VaultSession();
        var service = CreateUnlockService(session);

        Assert.Equal(UnlockResult.InvalidPassword, await service.UnlockAsync("wrong-example"));
        Assert.Equal(UnlockResult.Success, await service.UnlockAsync("correct-example"));

        using var finalContext = CreateContext();
        var stored = Assert.Single(finalContext.User.ToList());

        Assert.Equal("Site", VaultCrypto.Decrypt(session.GetKey(), stored.Site));
        Assert.Equal("secret", VaultCrypto.Decrypt(session.GetKey(), stored.Password));
    }

    private PWDbContext CreateContext()
    {
        var context = new PWDbContext(_path);
        context.Database.EnsureCreated();
        VaultSchemaInitializer.CreateVaultMetadataTableIfMissing(context);

        return context;
    }

    private VaultSession CreateUnlockedSession(string password)
    {
        var session = new VaultSession();
        session.Unlock(VaultCrypto.DeriveKey(password, VaultCrypto.CreateSalt(), 1000));

        return session;
    }

    private MainViewModel CreateViewModel(IVaultEntryService vaultService) => new(
        vaultService,
        Mock.Of<IClipboardService>(),
        TimeProvider.System,
        Mock.Of<IThemeService>(),
        new ColorThemeStore(Path.Combine(_directory, "themes")),
        new AppSettingsService(new AppSettingsStore(Path.Combine(_directory, "settings"))),
        Mock.Of<IVaultRekeyService>(),
        Mock.Of<IVaultBackupService>(),
        Mock.Of<IFilePickerService>());

    private UnlockService CreateUnlockService(IVaultSession session)
    {
        var factory = new PWDbContextFactory(_path);
        var context = factory.CreateDbContext();

        return new UnlockService(
            new VaultMetadataRepository(context),
            new UserRepository(context),
            new VaultKeyService(),
            new VaultDataMigrator(factory),
            session,
            TimeProvider.System);
    }

    private static User CreateLegacyEntry(string password)
    {
        var user = new User(
            LegacyVaultCrypto.Encrypt("GitHub", password),
            LegacyVaultCrypto.Encrypt("alice", password),
            LegacyVaultCrypto.Encrypt("secret", password));

        return user;
    }
}
