using System.Data.Common;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 慢查询日志
/// </summary>
internal sealed class SlowQueryInterceptor(IOptions<DatabaseDiagnosticsOptions> options, FrameworkMetrics metrics, ILogger<SlowQueryInterceptor> logger)
    : DbCommandInterceptor
{
    private const int MaxSqlLength = 4096;

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Observe(command, eventData, failed: false);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Observe(command, eventData, failed: false);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Observe(command, eventData, failed: false);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        Observe(command, eventData, failed: false);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Observe(command, eventData, failed: false);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        Observe(command, eventData, failed: false);
        return ValueTask.FromResult(result);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData) => Observe(command, eventData, failed: true);

    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        Observe(command, eventData, failed: true);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 只留语句类型：跳过开头的注释（EF 的 TagWith 会写在最前面）和空白，取第一个单词
    /// </summary>
    internal static string StatementKind(string commandText)
    {
        ArgumentNullException.ThrowIfNull(commandText);

        foreach (var line in commandText.AsSpan().EnumerateLines())
        {
            var trimmed = line.Trim();
            if (trimmed.IsEmpty || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var end = trimmed.IndexOfAny(" \t(");
            return (end < 0 ? trimmed : trimmed[..end]).ToString().ToUpperInvariant();
        }

        return "UNKNOWN";
    }

    private void Observe(DbCommand command, CommandEndEventData eventData, bool failed)
    {
        if (eventData.Duration.TotalMilliseconds < options.Value.SlowQueryMs)
        {
            return;
        }

        var module = eventData.Context is { } context ? ModuleNames.Of(context.GetType()) : "Unknown";
        metrics.SlowQuery(module);
        ObservabilityLog.SlowQuery(logger, module, eventData.Duration.TotalMilliseconds, failed, Describe(command.CommandText));
    }

    private string Describe(string commandText)
    {
        if (!options.Value.RecordSqlText)
        {
            return StatementKind(commandText);
        }

        return commandText.Length <= MaxSqlLength ? commandText : string.Concat(commandText.AsSpan(0, MaxSqlLength), "…");
    }
}
