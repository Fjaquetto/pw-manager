using Microsoft.EntityFrameworkCore;

namespace PWManager.Infra.Context.SQLite;

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

    // Old vaults were created with EnsureCreated, which never adds new tables,
    // so this idempotent guard runs on every open.
    public static void CreateVaultMetadataTableIfMissing(DbContext context) => context.Database.ExecuteSqlRaw(CreateVaultMetadataSql);
}
