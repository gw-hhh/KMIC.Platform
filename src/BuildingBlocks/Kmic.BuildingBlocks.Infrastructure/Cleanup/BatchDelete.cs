using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Cleanup;

// 按条件分批删除，每批独立提交直至清空
internal static class BatchDelete
{
    public static async Task<long> RunAsync(
        NpgsqlDataSource database, string table, string condition, IReadOnlyDictionary<string, object> parameters, int batchSize, CancellationToken ct)
    {
        long total = 0;
        while (true)
        {
            await using var command = database.CreateCommand(
                $"""DELETE FROM {table} WHERE "Id" IN (SELECT t."Id" FROM {table} t WHERE {condition} LIMIT @batch)""");
            command.Parameters.AddWithValue("batch", batchSize);
            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value);
            }

            var deleted = await command.ExecuteNonQueryAsync(ct);
            total += deleted;
            if (deleted < batchSize)
            {
                return total;
            }

            // 批次间短暂让出，减少对其他事务的锁竞争
            await Task.Yield();
        }
    }
}
