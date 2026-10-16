using Microsoft.EntityFrameworkCore;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
using PWManager.Infra.Context.SQLite;

namespace PWManager.Infra.Repository;

public sealed class VaultMetadataRepository(PWDbContext context) : IVaultMetadataRepository
{
    public Task<bool> AnyAsync() => context.VaultMetadata.AnyAsync();

    public Task<VaultMetadata?> GetAsync() => context.VaultMetadata.AsNoTracking().FirstOrDefaultAsync();

    public async Task AddAsync(VaultMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        await context.VaultMetadata.AddAsync(metadata);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }
}
