using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

/// <summary>
/// 计数表的运维入口
/// </summary>
/// <remarks>
/// 给运维程序用，应用本身取号走 INumberGenerator。导入旧数据后用它把计数抬到旧编号的最大序号，
/// 之后取到的新编号不会和旧编号重复
/// </remarks>
public static partial class NumberCounters
{
    /// <summary>
    /// 保证某个计数不小于给定值
    /// </summary>
    /// <remarks>
    /// 计数不存在就创建，已经更大就保持不变，只增不减：重复执行、或者和正在取号的应用同时执行都是安全的
    /// </remarks>
    /// <param name="database">有计数表写权限的账号的数据源</param>
    /// <param name="schema">计数表所在的 schema，即规则所属模块的名称</param>
    /// <param name="rule">编号规则</param>
    /// <param name="businessDate">旧编号所在的日期，用来确定周期；不重置的规则可以传任意日期</param>
    /// <param name="partition">分组键，不分组的规则传 null</param>
    /// <param name="value">要保证的最小序号，旧数据里这个周期与分组下的最大序号</param>
    /// <param name="time">时间</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>执行之后的计数值</returns>
    /// <exception cref="ArgumentException">分组键不符合规则、序号不是正数、schema 不是合法的模块名</exception>
    public static async Task<long> EnsureAtLeastAsync(
        NpgsqlDataSource database,
        string schema,
        NumberRule rule,
        DateOnly businessDate,
        string? partition,
        long value,
        TimeProvider time,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(time);
        if (!SchemaPattern().IsMatch(schema))
        {
            throw new ArgumentException($"“{schema}”不是合法的模块名。", nameof(schema));
        }

        if (value < 1)
        {
            throw new ArgumentException("序号要大于 0。", nameof(value));
        }

        var table = $"\"{schema}\".\"{FrameworkTables.NumberCounters}\"";
        await using var command = database.CreateCommand(
            $"""
            INSERT INTO {table} ("Rule", "PeriodKey", "PartitionKey", "LastValue", "UpdatedAt")
            VALUES (@rule, @period, @partition, @value, @now)
            ON CONFLICT ("Rule", "PeriodKey", "PartitionKey")
            DO UPDATE SET "LastValue" = GREATEST({table}."LastValue", EXCLUDED."LastValue"),
                          "UpdatedAt" = CASE WHEN EXCLUDED."LastValue" > {table}."LastValue" THEN EXCLUDED."UpdatedAt" ELSE {table}."UpdatedAt" END
            RETURNING "LastValue"
            """);
        command.Parameters.AddWithValue("rule", rule.Name);
        command.Parameters.AddWithValue("period", rule.PeriodKey(businessDate));
        command.Parameters.AddWithValue("partition", rule.PartitionKey(partition));
        command.Parameters.AddWithValue("value", value);
        command.Parameters.AddWithValue("now", time.GetUtcNow());
        return (long)(await command.ExecuteScalarAsync(ct))!;
    }

    [GeneratedRegex("^[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SchemaPattern();
}
