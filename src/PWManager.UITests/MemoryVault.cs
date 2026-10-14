using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Infra.Config;
using PWManager.Services;
using PWManager.Services.Interfaces;
using PWManager.UITests.Fakes;
using PWManager.ViewModels;

namespace PWManager.UITests;

internal sealed class MemoryVault : IUserApplication, IUserEncryptorService, IClipboardService
{
    public List<User> Users { get; } = new();

    public string? CopiedText
    { get; private set; }

    public Task<IEnumerable<User>> GetAllUsersAsync() => Task.FromResult<IEnumerable<User>>(Users.ToArray());

    public Task<User> GetUserByIdAsync(Guid id) => Task.FromResult(Users.FirstOrDefault(u => u.Id == id)!);

    public Task AddUserAsync(User user)
    {
        Users.Add(user);

        return Task.CompletedTask;
    }

    public Task UpdateUserAsync(User user)
    {
        Users.RemoveAll(u => u.Id == user.Id);
        Users.Add(user);

        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(User user)
    {
        Users.RemoveAll(u => u.Id == user.Id);

        return Task.CompletedTask;
    }

    public User EncryptUser(User user) => user;

    public User DecryptUser(User user) => user;

    public Task<bool> SetTextAsync(string text)
    {
        CopiedText = text;

        return Task.FromResult(true);
    }

    public Task<string?> TryGetTextAsync() => Task.FromResult<string?>(CopiedText);

    public Task ClearAsync()
    {
        CopiedText = null;

        return Task.CompletedTask;
    }

    public AppSettingsFake Settings { get; } = new();

    public RekeyStub Rekey { get; } = new();

    public BackupStub Backup { get; } = new();

    public FilePickerStub FilePicker { get; } = new();

    public MainViewModel CreateViewModel() => new(
        new VaultEntryService(this, this, TimeProvider.System),
        this,
        TimeProvider.System,
        new ThemeService(),
        new ColorThemeStore(ThemeDirectory),
        Settings,
        Rekey,
        Backup,
        FilePicker);

    private static string ThemeDirectory => Path.Combine(Path.GetTempPath(), "pwmanager-ui-themes-unused");

    public static MemoryVault Sample(int count = 7)
    {
        var vault = new MemoryVault();
        var names = new[]
        {
            "Figma",
            "GitHub",
            "Google",
            "Linear",
            "Notion",
            "Spotify",
            "Work account"
        };
        for (var i = 0; i < count; i++)
        {
            var site = names[i % names.Length] + (i >= names.Length ? $" {i:D4}" : "");
            var login = i % 2 == 0 ? "alex@example.com" : "alex.morgan";

            vault.Users.Add(new(site, login, "Example-password-42!")
            {
                CreationDate = new(2026, 2, 18, 9, 30, 0),
                LastUpdated = new(2026, 8, 28, 14, 15, 0)
            });
        }

        return vault;
    }
}
