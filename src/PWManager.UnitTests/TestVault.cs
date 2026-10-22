using Microsoft.Extensions.Time.Testing;
using Moq;
using PWManager.Application;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
using PWManager.Infra.Config;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;

namespace PWManager.UnitTests;

internal sealed class TestVault
{
    public Mock<IUserRepository> UserRepository { get; } = new();

    public Mock<IUserEncryptorService> Encryptor { get; } = new();

    public Mock<IClipboardService> Clipboard { get; } = new();

    public Mock<IVaultRekeyService> Rekey { get; } = new();

    public Mock<IVaultBackupService> Backup { get; } = new();

    public Mock<IFilePickerService> FilePicker { get; } = new();

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 9, 6, 12, 0, 0, TimeSpan.Zero));

    public AppSettingsService Settings { get; }

    public List<User> Users { get; } = new();

    public MainViewModel ViewModel { get; }

    public TestVault(params User[] users)
    {
        Users.AddRange(users);
        UserRepository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(() => Users.ToArray());
        UserRepository.Setup(repository => repository.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => Users.FirstOrDefault(u => u.Id == id)!);
        UserRepository.Setup(repository => repository.AddAsync(It.IsAny<User>())).Callback<User>(Users.Add).Returns(Task.CompletedTask);
        UserRepository.Setup(repository => repository.UpdateAsync(It.IsAny<User>())).Callback<User>(user =>
        {
            Users.RemoveAll(u => u.Id == user.Id);
            Users.Add(user);
        }).Returns(Task.CompletedTask);
        UserRepository.Setup(repository => repository.DeleteAsync(It.IsAny<User>())).Callback<User>(user => Users.RemoveAll(u => u.Id == user.Id)).Returns(Task.CompletedTask);
        Encryptor.Setup(e => e.EncryptUser(It.IsAny<User>())).Returns((User u) => u);
        Encryptor.Setup(e => e.DecryptUser(It.IsAny<User>())).Returns((User u) => u);
        Clipboard.Setup(c => c.SetTextAsync(It.IsAny<string>())).ReturnsAsync(true);
        Settings = new AppSettingsService(new AppSettingsStore(Path.Combine(Path.GetTempPath(), "pwmanager-tests-settings-" + Guid.NewGuid().ToString("N"))));
        ViewModel = new(
            new VaultEntryService(UserRepository.Object, Encryptor.Object, Time),
            Clipboard.Object,
            Time,
            Mock.Of<IThemeService>(),
            new ColorThemeStore(UnusedThemeDirectory),
            Settings,
            Rekey.Object,
            Backup.Object,
            FilePicker.Object,
            TestAppInfo.Create());
    }

    private static string UnusedThemeDirectory => Path.Combine(Path.GetTempPath(), "pwmanager-tests-no-themes");

    public void Fill(string site = "Personal account", string login = "name@example.com", string password = "  exact secret  ")
    {
        ViewModel.NewEntryCommand.Execute(null);
        ViewModel.Editor.Site = site;
        ViewModel.Editor.Login = login;
        ViewModel.Editor.Password = password;
    }
}
