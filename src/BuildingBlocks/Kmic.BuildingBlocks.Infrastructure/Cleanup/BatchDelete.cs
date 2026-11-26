using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Cleanup;

/// <summary>
/// 分批删除
/// </summary>
internal static class BatchDelete
{
    /// <summary>
    /// 按条件一批一批删，每批单独提交，删完为止
    /// </summary>
    /// <returns>删除的总行数</returns>
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

            // 批与批之间让一下，别的事务能拿到锁
            await Task.Yield();
        }
    }
}
