using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 幂等记录的读写：领取、接管、释放各用一个独立的短语句（不在业务事务中），完成标记在业务事务中写入
/// </summary>
/// <remarks>
/// 所有改写都带状态与令牌条件：不带条件的话，提交已生效而确认丢失时，失败路径会删掉已经是 Completed 的记录，
/// 客户端用原键重试就会重新执行（05-02 第 5.3 节）
/// </remarks>
/// <param name="dataSource">事务池；只在命令不持有业务连接时借用</param>
/// <param name="table">带引号的完整表名，如 "Sample"."IdempotencyRecord"</param>
internal sealed class IdempotencyStore(NpgsqlDataSource dataSource, string table)
{
    public const string Processing = "Processing";
    public const string Completed = "Completed";

    private const string Columns = """ "Id", "RequestHash", "Status", "LeaseToken", "LeaseExpiresAt", "ResponseBody", "ExpiresAt" """;

    public static IdempotencyStore For(ModuleDbContext context, NpgsqlDataSource dataSource)
    {
        var entityType = context.Model.FindEntityType(typeof(IdempotencyRecord))
            ?? throw new InvalidOperationException($"{context.Module} 模块的模型中没有幂等记录表。");
        return new IdempotencyStore(dataSource, $"\"{entityType.GetSchema()}\".\"{entityType.GetTableName()}\"");
    }

    public async Task<bool> TryClaimAsync(NewIdempotencyRecord record, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"""
            INSERT INTO {table} ("Id", "UserId", "RoleId", "Operation", "Key", "RequestHash", "Status", "LeaseToken", "LeaseExpiresAt", "CreatedAt", "ExpiresAt")
            VALUES (@id, @user, @role, @operation, @key, @hash, '{Processing}', @token, @lease, @now, @expires)
            ON CONFLICT ("UserId", "RoleId", "Operation", "Key") DO NOTHING
            """);
        command.Parameters.AddWithValue("id", record.Id);
        command.Parameters.AddWithValue("user", record.UserId);
        command.Parameters.AddWithValue("role", record.RoleId);
        command.Parameters.AddWithValue("operation", record.Operation);
        command.Parameters.AddWithValue("key", record.Key);
        command.Parameters.AddWithValue("hash", record.RequestHash);
        command.Parameters.AddWithValue("token", record.LeaseToken);
        command.Parameters.AddWithValue("lease", record.LeaseExpiresAt);
        command.Parameters.AddWithValue("now", record.CreatedAt);
        command.Parameters.AddWithValue("expires", record.ExpiresAt);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task<IdempotencySnapshot?> FindAsync(NewIdempotencyRecord record, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"""
            SELECT {Columns} FROM {table}
            WHERE "UserId" = @user AND "RoleId" = @role AND "Operation" = @operation AND "Key" = @key
            """);
        command.Parameters.AddWithValue("user", record.UserId);
        command.Parameters.AddWithValue("role", record.RoleId);
        command.Parameters.AddWithValue("operation", record.Operation);
        command.Parameters.AddWithValue("key", record.Key);
        return await ReadSingleAsync(command, ct);
    }

    public async Task<IdempotencySnapshot?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"""SELECT {Columns} FROM {table} WHERE "Id" = @id""");
        command.Parameters.AddWithValue("id", id);
        return await ReadSingleAsync(command, ct);
    }

    // 只有租约已到期、令牌仍是读到的那个时才接管，两个请求同时接管只有一个成功
    public async Task<bool> TryTakeOverAsync(IdempotencySnapshot existing, Guid newToken, DateTimeOffset leaseExpiresAt, DateTimeOffset now, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"""
            UPDATE {table} SET "LeaseToken" = @new, "LeaseExpiresAt" = @lease
            WHERE "Id" = @id AND "LeaseToken" = @old AND "Status" = '{Processing}' AND "LeaseExpiresAt" <= @now
            """);
        command.Parameters.AddWithValue("new", newToken);
        command.Parameters.AddWithValue("lease", leaseExpiresAt);
        command.Parameters.AddWithValue("id", existing.Id);
        command.Parameters.AddWithValue("old", existing.LeaseToken);
        command.Parameters.AddWithValue("now", now);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    // 提交中的事务持有这一行的行锁，删除会等它结束后再判断，不会把“正在提交”误判为“未提交”
    public async Task<bool> TryReleaseAsync(Guid id, Guid token, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"""DELETE FROM {table} WHERE "Id" = @id AND "Status" = '{Processing}' AND "LeaseToken" = @token""");
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("token", token);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task DeleteExpiredAsync(Guid id, DateTimeOffset now, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"""DELETE FROM {table} WHERE "Id" = @id AND "ExpiresAt" <= @now""");
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("now", now);
        await command.ExecuteNonQueryAsync(ct);
    }

    // 在业务事务中执行：完成标记与业务数据一起提交；令牌不符说明已被接管，整个命令回滚
    public async Task<bool> TryCompleteAsync(ModuleDbContext context, Guid id, Guid token, string? responseBody, DateTimeOffset now, CancellationToken ct)
    {
        var sql = $"""
            UPDATE {table} SET "Status" = '{Completed}', "ResponseBody" = @body, "CompletedAt" = @now
            WHERE "Id" = @id AND "LeaseToken" = @token AND "Status" = '{Processing}'
            """;
        var body = new NpgsqlParameter("body", NpgsqlDbType.Jsonb) { Value = (object?)responseBody ?? DBNull.Value };
        return await context.Database.ExecuteSqlRawAsync(
            sql,
            [body, new NpgsqlParameter("now", now), new NpgsqlParameter("id", id), new NpgsqlParameter("token", token)],
            ct) == 1;
    }

    private static async Task<IdempotencySnapshot?> ReadSingleAsync(NpgsqlCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new IdempotencySnapshot(
            reader.GetGuid(0),
            await reader.GetFieldValueAsync<byte[]>(1, ct),
            reader.GetString(2),
            reader.GetGuid(3),
            await reader.GetFieldValueAsync<DateTimeOffset>(4, ct),
            await reader.IsDBNullAsync(5, ct) ? null : reader.GetString(5),
            await reader.GetFieldValueAsync<DateTimeOffset>(6, ct));
    }
}
