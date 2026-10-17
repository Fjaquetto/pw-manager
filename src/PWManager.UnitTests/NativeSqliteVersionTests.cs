using Microsoft.Data.Sqlite;

namespace PWManager.UnitTests;

public class NativeSqliteVersionTests
{
    // CVE-2025-6965: SQLite before 3.50.2 lets aggregate terms outnumber the
    // available columns, corrupting memory. The bundled native library must
    // stay at or above that version, no matter what the package graph says.
    private static readonly Version MinimumPatchedVersion = new(3, 50, 2);

    [Fact]
    public void BundledSqlite_IsPatchedAgainstCve20256965()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT sqlite_version()";

        var version = new Version((string)command.ExecuteScalar()!);

        Assert.True(
            version >= MinimumPatchedVersion,
            $"The bundled SQLite is {version}, older than the patched {MinimumPatchedVersion}.");
    }
}
