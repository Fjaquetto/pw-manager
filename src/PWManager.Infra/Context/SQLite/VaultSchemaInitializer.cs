using Microsoft.EntityFrameworkCore;

namespace PWManager.Infra.Context.SQLite;

/// <summary>
/// Creates schema objects that were added after the database itself. The
/// application uses EnsureCreated (no EF migrations history), so databases
/// created by earlier versions need this idempotent guard.
/// </summary>
public static class VaultSchemaInitializer
{
    private const string CreateVaultMetadataSql = """
        CREATE TABLE IF NOT EXISTS "VaultMetadata" (
            "Id" INTEGER NOT NULL CONSTRAINT "PK_VaultMetadata" PRIMARY KEY,
            "FormatVersion" INTEGER NOT NULL,
            "Kdf" TEXT NOT NULL,
            "KdfIterations" INTEGER NOT NULL,
            "KdfSalt" TEXT NOT NULL,
            "Verifier" TEXT NOT NULL,
            "CreatedAt" TEXT NOT NULL
        );
        """;

    public static void EnsureUpToDate(DbContext context) => context.Database.ExecuteSqlRaw(CreateVaultMetadataSql);
}
