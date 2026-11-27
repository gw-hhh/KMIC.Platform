using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Cleanup;

/// <summary>只追加表过期数据清理工具（由迁移账号执行）</summary>
public static class AppendOnlyPurge
{
    public const string AuditLog = """
        "Operations"."AuditLog"
        """;

    public const string SecurityEvent = """
        "Identity"."SecurityEvent"
        """;

    /// <summary>支持清理的完整表名列表</summary>
    public static IReadOnlyList<string> Tables { get; } = [AuditLog, SecurityEvent];

    /// <summary>命令行参数到数据库表名的映射</summary>
    public static IReadOnlyDictionary<string, string> Targets { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["audit-log"] = AuditLog,
        ["security-event"] = SecurityEvent,
    };

    /// <summary>分批删除早于 before 的记录（每批独立提交），表未创建时返回 null</summary>
    public static async Task<long?> RunAsync(NpgsqlDataSource database, string table, DateTimeOffset before, int batchSize, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(database);
        if (!Tables.Contains(table, StringComparer.Ordinal))
        {
            throw new ArgumentException($"{table} 不是可以清理的表，可选：{string.Join("、", Tables)}。", nameof(table));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        await using (var exists = database.CreateCommand("SELECT to_regclass(@name) IS NOT NULL"))
        {
            exists.Parameters.AddWithValue("name", table);
            if (!(bool)(await exists.ExecuteScalarAsync(ct))!)
            {
                return null;
            }
        }

        return await BatchDelete.RunAsync(
            database, table, """t."OccurredAt" < @before""", new Dictionary<string, object>(StringComparer.Ordinal) { ["before"] = before }, batchSize, ct);
    }
}
