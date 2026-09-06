using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PWManager.Application;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Enums;
using PWManager.Infra.Context.SQLite;
using PWManager.Infra.Repository;
using PWManager.Infra.Services;
using PWManager.Services;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;

namespace PWManager.UnitTests;

[Collection("Vault encryption")]
public class PersistenceTests
{
    [Fact]
    public async Task TemporarySqlite_CreateEditReopenDelete_PreservesEncryptionAndDates()
    {
        var directory = Path.Combine(Path.GetTempPath(), "pwmanager-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "vault.db");
        var previousPassword = EncryptorService.EncryptorPassword;
        EncryptorService.EncryptorPassword = "Only-a-test-master-password";

        try
        {
            Guid id;
            DateTime created;
            using (var context = new PWDbContext(path))
            {
                await context.Database.EnsureCreatedAsync();
                var application = new UserApplication(new UserRepository(context));
                var vaultService = new VaultEntryService(application, new UserEncryptorService(), TimeProvider.System);
                using var viewModel = new MainViewModel(vaultService, Mock.Of<IClipboardService>(), TimeProvider.System);

                await viewModel.LoadEntriesAsync();
                viewModel.NewEntryCommand.Execute(null);
                viewModel.Editor.Site = "Test account";
                viewModel.Editor.Login = "alex@example.com";
                viewModel.Editor.Password = "  keep these spaces  ";

                await viewModel.SaveEntryCommand.ExecuteAsync(null);

                Assert.False(viewModel.Notification.IsError);
                id = viewModel.SelectedEntry!.Id;
                created = viewModel.SelectedEntry.CreationDate;
                var encrypted = await application.GetUserByIdAsync(id);

                Assert.NotEqual("Test account", encrypted.Site);
                Assert.NotEqual("alex@example.com", encrypted.Login);
                Assert.NotEqual("  keep these spaces  ", encrypted.Password);
                viewModel.EditEntryCommand.Execute(null);
                viewModel.Editor.Login = "updated@example.com";

                await viewModel.SaveEntryCommand.ExecuteAsync(null);

                Assert.Equal(created, viewModel.SelectedEntry!.CreationDate);
            }

            using (var context = new PWDbContext(path))
            {
                var application = new UserApplication(new UserRepository(context));
                var vaultService = new VaultEntryService(application, new UserEncryptorService(), TimeProvider.System);
                using var viewModel = new MainViewModel(vaultService, Mock.Of<IClipboardService>(), TimeProvider.System);

                await viewModel.LoadEntriesAsync();
                var restored = Assert.Single(viewModel.Entries);

                Assert.Equal(id, restored.Id);
                Assert.Equal(created, restored.CreationDate);
                Assert.Equal("updated@example.com", restored.Login);
                Assert.Equal("  keep these spaces  ", restored.Password);
                viewModel.SelectedEntry = restored;
                viewModel.RequestDeleteCommand.Execute(null);

                await viewModel.ConfirmDeleteCommand.ExecuteAsync(null);

                Assert.Empty(await application.GetAllUsersAsync());
            }
        }
        finally
        {
            EncryptorService.EncryptorPassword = previousPassword;
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task UnlockService_EmptyVaultKeepsExistingBehavior()
    {
        var previousPassword = EncryptorService.EncryptorPassword;

        try
        {
            var application = new Mock<IUserApplication>();
            application.Setup(a => a.GetAllUsersAsync()).ReturnsAsync(Array.Empty<User>());
            using var provider = new ServiceCollection()
                .AddSingleton(application.Object)
                .AddSingleton<IUserEncryptorService, UserEncryptorService>()
                .BuildServiceProvider();
            var service = new UnlockService(provider.GetRequiredService<IServiceScopeFactory>());

            Assert.Equal(UnlockResult.Success, await service.UnlockAsync("example"));
            Assert.Equal("example", EncryptorService.EncryptorPassword);
        }
        finally
        {
            EncryptorService.EncryptorPassword = previousPassword;
        }
    }

    [Fact]
    public async Task UnlockService_ValidatesExistingCiphertext_WithoutMutatingIt()
    {
        var previousPassword = EncryptorService.EncryptorPassword;

        try
        {
            EncryptorService.EncryptorPassword = "correct-example";
            var user = new UserEncryptorService().EncryptUser(new("Site", "login", "secret"));
            var cipher = user.Password;
            var application = new Mock<IUserApplication>();
            application.Setup(a => a.GetAllUsersAsync()).ReturnsAsync(new[] { user });
            using var provider = new ServiceCollection()
                .AddSingleton(application.Object)
                .AddSingleton<IUserEncryptorService, UserEncryptorService>()
                .BuildServiceProvider();
            var service = new UnlockService(provider.GetRequiredService<IServiceScopeFactory>());

            Assert.Equal(UnlockResult.InvalidPassword, await service.UnlockAsync("wrong-example"));
            Assert.Empty(EncryptorService.EncryptorPassword);
            Assert.Equal(UnlockResult.Success, await service.UnlockAsync("correct-example"));
            Assert.Equal(cipher, user.Password);
        }
        finally
        {
            EncryptorService.EncryptorPassword = previousPassword;
        }
    }
}
