using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

/// <summary>
/// 业务编号生成
/// </summary>
/// <remarks>
/// 一条语句完成“没有就创建、有就加一”，并把计数行锁到事务结束：同一个规则的并发取号排队，编号不会重复。
/// 周期键和日期段用同一个时刻算，跨午夜的请求里两者不会不一致
/// </remarks>
internal sealed class NumberGenerator(
    NumberRuleCatalog rules,
    IBusinessCalendar calendar,
    TimeProvider time,
    IExecutionContext execution,
    CommandTransaction transaction,
    ModuleWriteContexts contexts,
    ModuleFrameworkTables tables,
    ILogger<NumberGenerator> logger) : INumberGenerator
{
    public async ValueTask<string> NextAsync(string rule, string? partition = null, CancellationToken ct = default)
    {
        var definition = rules.Get(rule);
        if (!execution.InCommand || transaction.Module is null)
        {
            throw new InvalidOperationException($"取编号 {rule} 只能在命令里调用，计数要和单据一起提交。");
        }

        if (!string.Equals(transaction.Module, definition.Module, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"编号规则 {rule} 属于 {definition.Module} 模块，只能由该模块的命令取号。");
        }

        var now = time.GetUtcNow();
        var businessDate = calendar.DateOf(now);
        var partitionKey = definition.PartitionKey(partition);
        var dbTransaction = contexts.Get(definition.Module).Database.CurrentTransaction?.GetDbTransaction()
            ?? throw new InvalidOperationException("命令的事务还没有开始。");

        await using var command = dbTransaction.Connection!.CreateCommand();
        command.Transaction = dbTransaction;
        var table = tables.For(definition.Module).NumberCounters;
        command.CommandText =
            $"""
            INSERT INTO {table} ("Rule", "PeriodKey", "PartitionKey", "LastValue", "UpdatedAt")
            VALUES (@rule, @period, @partition, 1, @now)
            ON CONFLICT ("Rule", "PeriodKey", "PartitionKey")
            DO UPDATE SET "LastValue" = {table}."LastValue" + 1, "UpdatedAt" = EXCLUDED."UpdatedAt"
            RETURNING "LastValue"
            """;
        command.Parameters.Add(new NpgsqlParameter("rule", rule));
        command.Parameters.Add(new NpgsqlParameter("period", definition.PeriodKey(businessDate)));
        command.Parameters.Add(new NpgsqlParameter("partition", partitionKey));
        command.Parameters.Add(new NpgsqlParameter("now", now));

        var sequence = (long)(await command.ExecuteScalarAsync(ct))!;
        if (sequence == definition.WarningThreshold)
        {
            NumberingLog.SequenceNearLimit(logger, rule, sequence, definition.SequenceDigits);
        }

        return definition.Render(businessDate, sequence, partition);
    }
}
