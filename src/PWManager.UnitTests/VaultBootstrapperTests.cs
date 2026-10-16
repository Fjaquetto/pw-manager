using Moq;
using PWManager.Application;
using PWManager.Application.DataContracts;
using PWManager.Application.Enums;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;

namespace PWManager.UnitTests;

public class VaultBootstrapperTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pwmanager-bootstrap-" + Guid.NewGuid().ToString("N"));
    private readonly string _path;
    private readonly PWDbContextFactory _factory;

    public VaultBootstrapperTests()
    {
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "vault.db");
        _factory = new PWDbContextFactory(_path);
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
    public async Task InitializeAsync_FreshVault_CreatesSchemaAndReportsNew()
    {
        var status = await CreateBootstrapper().InitializeAsync();

        Assert.Equal(VaultStatus.New, status);
        Assert.True(File.Exists(_path));

        var tables = ReadTableNames();

        Assert.Contains("User", tables);
        Assert.Contains("VaultMetadata", tables);
    }

    [Fact]
    public async Task InitializeAsync_ExistingVault_ReportsExisting()
    {
        await CreateBootstrapper().InitializeAsync();
        CreateVault("correct-example");

        var status = await CreateBootstrapper().InitializeAsync();

        Assert.Equal(VaultStatus.Existing, status);
    }

    [Fact]
    public async Task InitializeAsync_VaultWithEntriesButNoMetadata_ReportsExisting()
    {
        using (var context = _factory.CreateDbContext())
        {
            context.Database.EnsureCreated();
            context.User.Add(new User("site", "login", "password"));
            context.SaveChanges();
        }

        var status = await CreateBootstrapper().InitializeAsync();

        Assert.Equal(VaultStatus.Existing, status);
    }

    [Fact(Timeout = 10000)]
    public async Task InitializeAsync_DoesNotRunOnTheCallingThread()
    {
        var callerThread = Environment.CurrentManagedThreadId;
        using var gate = new ManualResetEventSlim(false);
        var workerThread = 0;

        var unlockService = new Mock<IUnlockService>();
        unlockService
            .Setup(service => service.GetStatusAsync())
            .Returns(() =>
            {
                workerThread = Environment.CurrentManagedThreadId;
                gate.Wait();

                return Task.FromResult(VaultStatus.Existing);
            });

        var task = CreateBootstrapper(unlockService.Object).InitializeAsync();

        Assert.False(task.IsCompleted);

        gate.Set();

        Assert.Equal(VaultStatus.Existing, await task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.NotEqual(callerThread, workerThread);
    }

    private VaultBootstrapper CreateBootstrapper(IUnlockService? unlockService = null) =>
        new(new SqliteVaultSchemaInitializer(_factory), unlockService ?? CreateUnlockService());

    private UnlockService CreateUnlockService()
    {
        var context = _factory.CreateDbContext();

        return new UnlockService(
            new VaultMetadataRepository(context),
            new UserRepository(context),
            new VaultKeyService(),
            new VaultDataMigrator(_factory),
            new VaultSession(),
            TimeProvider.System);
    }

    private void CreateVault(string password)
    {
        var result = CreateUnlockService().UnlockAsync(password).GetAwaiter().GetResult();

        Assert.Equal(UnlockResult.Success, result);
    }

    private List<string> ReadTableNames()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";

        var names = new List<string>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }
}
