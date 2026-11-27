using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

// 模拟提交阶段连接故障；仅命中执行过指定 SQL 的事务，每次布置仅生效一次
internal sealed class CommitFaults : DbTransactionInterceptor, IDbCommandInterceptor
{
    private readonly Lock _gate = new();
    private readonly ConditionalWeakTable<DbTransaction, object> _matched = [];
    private Fault? _armed;

    // 提交已生效但未收到确认（occurrence 指定第几次命中生效）
    public void LoseAcknowledgement(string sql, int occurrence = 1) => Arm(new Fault(sql, occurrence, AfterCommit: true));

    // 提交未送达数据库连接即断开并回滚
    public void DropBeforeCommit(string sql) => Arm(new Fault(sql, 1, AfterCommit: false));

    public bool Fired { get; private set; }

    // Npgsql 在同一个连接上复用事务对象，新事务开始时清掉上一个事务留下的标记
    public override ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _matched.Remove(result);
        }

        return ValueTask.FromResult(result);
    }

    public override ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        if (TakeFault(transaction, afterCommit: false))
        {
            throw ConnectionLost("提交还没发出，连接已断开");
        }

        return ValueTask.FromResult(result);
    }

    public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default) =>
        TakeFault(transaction, afterCommit: true) ? throw ConnectionLost("提交后连接断开，没有收到确认") : Task.CompletedTask;

    ValueTask<InterceptionResult<DbDataReader>> IDbCommandInterceptor.ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken)
    {
        Observe(command);
        return ValueTask.FromResult(result);
    }

    ValueTask<InterceptionResult<int>> IDbCommandInterceptor.NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken)
    {
        Observe(command);
        return ValueTask.FromResult(result);
    }

    ValueTask<InterceptionResult<object>> IDbCommandInterceptor.ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken)
    {
        Observe(command);
        return ValueTask.FromResult(result);
    }

    // Npgsql 在连接断开时抛出的就是带 IOException 的 NpgsqlException
    private static NpgsqlException ConnectionLost(string message) => new($"模拟：{message}", new IOException("连接已断开"));

    private void Arm(Fault fault)
    {
        lock (_gate)
        {
            _armed = fault;
            Fired = false;
        }
    }

    private void Observe(DbCommand command)
    {
        lock (_gate)
        {
            if (_armed is { } fault && command.Transaction is { } transaction && command.CommandText.Contains(fault.Sql, StringComparison.Ordinal))
            {
                _matched.AddOrUpdate(transaction, fault);
            }
        }
    }

    private bool TakeFault(DbTransaction transaction, bool afterCommit)
    {
        lock (_gate)
        {
            if (_armed is not { } fault || fault.AfterCommit != afterCommit || !_matched.TryGetValue(transaction, out var matched) || !ReferenceEquals(matched, fault))
            {
                return false;
            }

            if (--fault.Remaining > 0)
            {
                return false;
            }

            _armed = null;
            Fired = true;
            return true;
        }
    }

    private sealed record Fault(string Sql, int Occurrence, bool AfterCommit)
    {
        public int Remaining { get; set; } = Occurrence;
    }
}
