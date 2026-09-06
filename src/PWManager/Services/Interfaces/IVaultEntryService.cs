using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PWManager.Domain.Model;

namespace PWManager.Services.Interfaces;

public interface IVaultEntryService
{
    Task<IReadOnlyList<User>> LoadAsync();

    Task<User?> SaveAsync(Guid? id, string site, string login, string password);

    Task<bool> DeleteAsync(Guid id);
}
