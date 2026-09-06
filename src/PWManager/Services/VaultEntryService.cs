using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public sealed class VaultEntryService(
    IUserApplication application,
    IUserEncryptorService encryptor,
    TimeProvider timeProvider) : IVaultEntryService
{
    public async Task<IReadOnlyList<User>> LoadAsync()
    {
        var encryptedEntries = await application.GetAllUsersAsync();

        return encryptedEntries
            .Select(entry => encryptor.DecryptUser(CopyEntry(entry)))
            .ToArray();
    }

    public async Task<User?> SaveAsync(Guid? id, string site, string login, string password)
    {
        var entry = id.HasValue
            ? await LoadEntryForUpdateAsync(id.Value)
            : CreateEntry(site, login, password);

        if (entry is null)
        {
            return null;
        }

        entry.Site = site;
        entry.Login = login;
        entry.Password = password;
        entry.LastUpdated = timeProvider.GetLocalNow().DateTime;
        var encryptedEntry = encryptor.EncryptUser(CopyEntry(entry));

        if (id.HasValue)
        {
            await application.UpdateUserAsync(encryptedEntry);
        }
        else
        {
            await application.AddUserAsync(encryptedEntry);
        }

        return entry;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entry = await application.GetUserByIdAsync(id);

        if (entry is null)
        {
            return false;
        }

        await application.DeleteUserAsync(entry);

        return true;
    }

    private async Task<User?> LoadEntryForUpdateAsync(Guid id)
    {
        var encryptedEntry = await application.GetUserByIdAsync(id);

        return encryptedEntry is null ? null : encryptor.DecryptUser(CopyEntry(encryptedEntry));
    }

    private User CreateEntry(string site, string login, string password)
    {
        return new User(site, login, password)
        {
            CreationDate = timeProvider.GetLocalNow().DateTime
        };
    }

    private static User CopyEntry(User entry)
    {
        return new User
        {
            Id = entry.Id,
            Site = entry.Site,
            Login = entry.Login,
            Password = entry.Password,
            CreationDate = entry.CreationDate,
            LastUpdated = entry.LastUpdated
        };
    }
}
