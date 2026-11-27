using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Numbering;

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
            throw new InvalidOperationException($"生成编号 {rule} 仅限在命令中调用，以保证计数与单据在同一事务中提交。");
        }

        if (!string.Equals(transaction.Module, definition.Module, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"编号规则 {rule} 归属 {definition.Module} 模块，仅允许该模块的命令调用。");
        }

        var now = time.GetUtcNow();
        var businessDate = calendar.DateOf(now);
        var partitionKey = definition.PartitionKey(partition);
        var dbTransaction = contexts.Get(definition.Module).Database.CurrentTransaction?.GetDbTransaction()
            ?? throw new InvalidOperationException("命令事务尚未开启。");

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
