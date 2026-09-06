using Microsoft.Extensions.Time.Testing;
using Moq;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Services;
using PWManager.Services.Interfaces;
using PWManager.ViewModels;

namespace PWManager.UnitTests;

internal sealed class TestVault
{
    public Mock<IUserApplication> Application { get; } = new();

    public Mock<IUserEncryptorService> Encryptor { get; } = new();

    public Mock<IClipboardService> Clipboard { get; } = new();

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 9, 6, 12, 0, 0, TimeSpan.Zero));

    public List<User> Users { get; } = new();

    public MainViewModel ViewModel
    { get; }

    public TestVault(params User[] users)
    {
        Users.AddRange(users);
        Application.Setup(a => a.GetAllUsersAsync()).ReturnsAsync(() => Users.ToArray());
        Application.Setup(a => a.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Guid id) => Users.FirstOrDefault(u => u.Id == id)!);
        Application.Setup(a => a.AddUserAsync(It.IsAny<User>())).Callback<User>(Users.Add).Returns(Task.CompletedTask);
        Application.Setup(a => a.UpdateUserAsync(It.IsAny<User>())).Callback<User>(user =>
        {
            Users.RemoveAll(u => u.Id == user.Id);
            Users.Add(user);
        }).Returns(Task.CompletedTask);
        Application.Setup(a => a.DeleteUserAsync(It.IsAny<User>())).Callback<User>(user => Users.RemoveAll(u => u.Id == user.Id)).Returns(Task.CompletedTask);
        Encryptor.Setup(e => e.EncryptUser(It.IsAny<User>())).Returns((User u) => u);
        Encryptor.Setup(e => e.DecryptUser(It.IsAny<User>())).Returns((User u) => u);
        Clipboard.Setup(c => c.SetTextAsync(It.IsAny<string>())).ReturnsAsync(true);
        ViewModel = new(new VaultEntryService(Application.Object, Encryptor.Object, Time), Clipboard.Object, Time);
    }

    public void Fill(string site = "Personal account", string login = "name@example.com", string password = "  exact secret  ")
    {
        ViewModel.NewEntryCommand.Execute(null);
        ViewModel.Editor.Site = site;
        ViewModel.Editor.Login = login;
        ViewModel.Editor.Password = password;
    }
}
