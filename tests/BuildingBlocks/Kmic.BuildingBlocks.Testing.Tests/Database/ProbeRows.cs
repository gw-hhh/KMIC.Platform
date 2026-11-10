using Npgsql;

namespace Kmic.BuildingBlocks.Testing.Tests.Database;

internal static class ProbeRows
{
    public static async Task InsertAsync(NpgsqlDataSource dataSource, string owner, int count, CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            await using var command = dataSource.CreateCommand("""INSERT INTO "Probe" ("Id", "Owner") VALUES ($1, $2)""");
            command.Parameters.Add(new NpgsqlParameter { Value = Guid.CreateVersion7() });
            command.Parameters.Add(new NpgsqlParameter { Value = owner });
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public static async Task<IReadOnlyList<string>> OwnersAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("""SELECT "Owner" FROM "Probe" """);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var owners = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
        {
            owners.Add(reader.GetString(0));
        }

        return owners;
    }
}
