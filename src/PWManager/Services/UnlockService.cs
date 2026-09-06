using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.InfraService;
using PWManager.Domain.Model;
using PWManager.Enums;
using PWManager.Infra.Services;
using PWManager.Services.Interfaces;

namespace PWManager.Services;

public sealed class UnlockService(IServiceScopeFactory scopeFactory) : IUnlockService
{
    public async Task<UnlockResult> UnlockAsync(string password)
    {
        EncryptorService.EncryptorPassword = password;

        try
        {
            await ValidatePasswordAgainstStoredEntryAsync();

            return UnlockResult.Success;
        }
        catch (CryptographicException)
        {
            EncryptorService.EncryptorPassword = string.Empty;

            return UnlockResult.InvalidPassword;
        }
        catch (Exception)
        {
            EncryptorService.EncryptorPassword = string.Empty;

            return UnlockResult.Unavailable;
        }
    }

    private async Task ValidatePasswordAgainstStoredEntryAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var application = scope.ServiceProvider.GetRequiredService<IUserApplication>();
        var encryptor = scope.ServiceProvider.GetRequiredService<IUserEncryptorService>();
        var storedEntry = (await application.GetAllUsersAsync()).FirstOrDefault();

        if (storedEntry is null)
        {
            return;
        }

        var validationCopy = new User
        {
            Id = storedEntry.Id,
            Site = storedEntry.Site,
            Login = storedEntry.Login,
            Password = storedEntry.Password,
            CreationDate = storedEntry.CreationDate,
            LastUpdated = storedEntry.LastUpdated
        };
        encryptor.DecryptUser(validationCopy);
    }
}
