using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

/// <summary>业务编号计数运维入口，用于在导入历史数据后提升起始序号</summary>
public static partial class NumberCounters
{
    /// <summary>将指定计数提升至不低于给定值（只增不减），返回更新后的计数值</summary>
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
            throw new ArgumentException("序号必须大于 0。", nameof(value));
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
