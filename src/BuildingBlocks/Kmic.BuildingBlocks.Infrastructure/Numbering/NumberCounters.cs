using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

/// <summary>
/// 计数表的运维入口
/// </summary>
/// <remarks>
/// 导入旧数据后用它把计数抬到旧编号的最大序号
/// </remarks>
public static partial class NumberCounters
{
    /// <summary>
    /// 保证某个计数不小于给定值，只增不减
    /// </summary>
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
