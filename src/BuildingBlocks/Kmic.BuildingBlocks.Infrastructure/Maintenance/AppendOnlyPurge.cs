using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Maintenance;

/// <summary>
/// 只追加表的过期数据清理，由迁移账号执行
/// </summary>
public static class AppendOnlyPurge
{
    /// <summary>
    /// 审计日志
    /// </summary>
    public const string AuditLog = """
        "Operations"."AuditLog"
        """;

    /// <summary>
    /// 安全事件
    /// </summary>
    public const string SecurityEvent = """
        "Identity"."SecurityEvent"
        """;

    /// <summary>
    /// 可以清理的表，名称带 schema
    /// </summary>
    public static IReadOnlyList<string> Tables { get; } = [AuditLog, SecurityEvent];

    /// <summary>
    /// 命令行里用的名称对应的表
    /// </summary>
    public static IReadOnlyDictionary<string, string> Targets { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["audit-log"] = AuditLog,
        ["security-event"] = SecurityEvent,
    };

    /// <summary>
    /// 分批删除发生时间早于截止时间的行，每批单独提交
    /// </summary>
    /// <param name="table">表名，必须是 <see cref="Tables"/> 里的一个</param>
    /// <returns>删除的行数；表还不存在（对应模块尚未建立）时为 null</returns>
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
