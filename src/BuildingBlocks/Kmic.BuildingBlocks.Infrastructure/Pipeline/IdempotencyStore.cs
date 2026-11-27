using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

// 所有改写均须附带状态与租约令牌条件
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

    // 仅当占用已到期且令牌未变时才允许接管，保证并发接管时只有一个请求能成功
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

    // 若事务正在提交，行锁会阻塞本次删除直至提交完成，从而避免将“正在提交”误判为“未提交”
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

    // 在业务事务内写入完成标记；若令牌不匹配则说明已被其他请求接管，触发整条命令回滚
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
