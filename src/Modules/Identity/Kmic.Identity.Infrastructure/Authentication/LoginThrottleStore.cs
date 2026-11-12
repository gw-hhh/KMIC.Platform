using System.Data.Common;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Application.Authentication;
using Npgsql;

namespace Kmic.Identity.Infrastructure.Authentication;

/// <summary>
/// 登录失败计数的存储：每次操作用独立事务提交，登录命令回滚也不影响
/// </summary>
internal sealed class LoginThrottleStore(IIndependentDatabase database, LoginThrottlePolicy policy) : ILoginThrottleStore
{
    private const string Select = """
        SELECT "FailureCount", "LockLevel", "NextAllowedAt", "LockedUntil", "LastFailedAt"
        FROM "Identity"."LoginThrottle" WHERE "LoginNameHash" = @hash
        """;

    public async Task<LoginThrottleState?> GetAsync(string loginNameHash, CancellationToken ct) =>
        await database.InTransactionAsync(async (connection, transaction, token) => await ReadAsync(connection, transaction, loginNameHash, forUpdate: false, token), ct);

    public async Task<LoginThrottleState> RecordFailureAsync(string loginNameHash, DateTimeOffset now, CancellationToken ct) =>
        await database.InTransactionAsync(
            async (connection, transaction, token) =>
            {
                // 没有这一行时先插一个“空”行再锁定：两个并发的第一次失败不会各插一行，也不会丢计数
                await using (var ensure = Command(connection, transaction, """
                    INSERT INTO "Identity"."LoginThrottle" ("LoginNameHash", "FailureCount", "LockLevel", "LastFailedAt")
                    VALUES (@hash, 0, 0, @now) ON CONFLICT ("LoginNameHash") DO NOTHING
                    """))
                {
                    ensure.Parameters.Add(new NpgsqlParameter("hash", loginNameHash));
                    ensure.Parameters.Add(new NpgsqlParameter("now", now));
                    await ensure.ExecuteNonQueryAsync(token);
                }

                var previous = await ReadAsync(connection, transaction, loginNameHash, forUpdate: true, token);
                var next = policy.OnFailure(previous is { FailureCount: 0 } ? null : previous, now);
                await using var update = Command(connection, transaction, """
                    UPDATE "Identity"."LoginThrottle"
                    SET "FailureCount" = @count, "LockLevel" = @level, "NextAllowedAt" = @next, "LockedUntil" = @locked, "LastFailedAt" = @now
                    WHERE "LoginNameHash" = @hash
                    """);
                update.Parameters.Add(new NpgsqlParameter("count", next.FailureCount));
                update.Parameters.Add(new NpgsqlParameter("level", next.LockLevel));
                update.Parameters.Add(new NpgsqlParameter("next", (object?)next.NextAllowedAt ?? DBNull.Value));
                update.Parameters.Add(new NpgsqlParameter("locked", (object?)next.LockedUntil ?? DBNull.Value));
                update.Parameters.Add(new NpgsqlParameter("now", next.LastFailedAt));
                update.Parameters.Add(new NpgsqlParameter("hash", loginNameHash));
                await update.ExecuteNonQueryAsync(token);
                return next;
            },
            ct);

    public async Task ClearAsync(string loginNameHash, CancellationToken ct) =>
        await database.InTransactionAsync(
            async (connection, transaction, token) =>
            {
                await using var delete = Command(connection, transaction, """DELETE FROM "Identity"."LoginThrottle" WHERE "LoginNameHash" = @hash""");
                delete.Parameters.Add(new NpgsqlParameter("hash", loginNameHash));
                return await delete.ExecuteNonQueryAsync(token);
            },
            ct);

    private static async Task<LoginThrottleState?> ReadAsync(DbConnection connection, DbTransaction transaction, string loginNameHash, bool forUpdate, CancellationToken ct)
    {
        await using var command = Command(connection, transaction, forUpdate ? Select + " FOR UPDATE" : Select);
        command.Parameters.Add(new NpgsqlParameter("hash", loginNameHash));
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        var failureCount = await reader.GetFieldValueAsync<int>(0, ct);
        var lockLevel = await reader.GetFieldValueAsync<int>(1, ct);
        var nextAllowedAt = await reader.IsDBNullAsync(2, ct) ? (DateTimeOffset?)null : await reader.GetFieldValueAsync<DateTimeOffset>(2, ct);
        var lockedUntil = await reader.IsDBNullAsync(3, ct) ? (DateTimeOffset?)null : await reader.GetFieldValueAsync<DateTimeOffset>(3, ct);
        var lastFailedAt = await reader.GetFieldValueAsync<DateTimeOffset>(4, ct);
        return new LoginThrottleState(failureCount, lockLevel, nextAllowedAt, lockedUntil, lastFailedAt);
    }

    private static DbCommand Command(DbConnection connection, DbTransaction transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }
}
