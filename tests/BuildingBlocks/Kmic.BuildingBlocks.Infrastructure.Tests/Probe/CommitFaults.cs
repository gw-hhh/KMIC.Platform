using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 提交时的连接故障
/// </summary>
/// <remarks>
/// 只命中执行过指定 SQL 的事务，同一次操作里领取、写执行记录等其他提交不受影响。每次布置只生效一次
/// </remarks>
internal sealed class CommitFaults : DbTransactionInterceptor, IDbCommandInterceptor
{
    private readonly Lock _gate = new();
    private readonly ConditionalWeakTable<DbTransaction, object> _matched = [];
    private Fault? _armed;

    /// <summary>
    /// 提交已在数据库生效，但调用方收不到确认
    /// </summary>
    /// <param name="sql">事务里执行过的 SQL 包含这段文字时才命中</param>
    /// <param name="occurrence">第几次命中时生效</param>
    public void LoseAcknowledgement(string sql, int occurrence = 1) => Arm(new Fault(sql, occurrence, AfterCommit: true));

    /// <summary>
    /// 提交还没发到数据库连接就断了，事务随后回滚
    /// </summary>
    public void DropBeforeCommit(string sql) => Arm(new Fault(sql, 1, AfterCommit: false));

    /// <summary>
    /// 布置的故障是否已经触发
    /// </summary>
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
