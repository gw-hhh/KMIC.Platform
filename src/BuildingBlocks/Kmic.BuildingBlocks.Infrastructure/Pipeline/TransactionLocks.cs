using System.Globalization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

internal sealed class TransactionLocks(IExecutionContext execution, CommandTransaction transaction, ModuleWriteContexts contexts) : ITransactionLocks
{
    private const string TimeoutSetting = "lock_timeout";

    public async Task<Result> AcquireSharedAsync(string scope, IReadOnlyCollection<Guid> ids, TimeSpan timeout, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentNullException.ThrowIfNull(ids);
        if (!execution.InCommand || transaction.Module is not { } module)
        {
            throw new InvalidOperationException("事务级咨询锁仅限在命令事务中获取，以随事务提交或回滚自动释放。");
        }

        if (ids.Count == 0)
        {
            return Result.Success();
        }

        var keys = ids.Distinct().Select(id => LockKeys.Of(scope, id)).Distinct().Order().ToList();
        var database = contexts.Get(module).Database;

        // 超时设置仅在获取锁期间生效，取锁后立即恢复原值，避免影响同一事务内的后续语句
        var previous = (await database.SqlQuery<string>($"SELECT current_setting('lock_timeout') AS \"Value\"").ToListAsync(ct)).Single();
        await database.ExecuteSqlAsync($"SELECT set_config({TimeoutSetting}, {timeout.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture) + "ms"}, true)", ct);
        try
        {
            foreach (var key in keys)
            {
                await database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock_shared({key})", ct);
            }
        }
        catch (Exception exception) when (IsLockTimeout(exception))
        {
            return CommonErrors.TransientConflict;
        }

        await database.ExecuteSqlAsync($"SELECT set_config({TimeoutSetting}, {previous}, true)", ct);
        return Result.Success();
    }

    public async Task<IReadOnlySet<Guid>> TryAcquireExclusiveAsync(string scope, IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentNullException.ThrowIfNull(ids);
        if (!execution.InCommand || transaction.Module is not { } module)
        {
            throw new InvalidOperationException("事务级咨询锁仅限在命令事务中获取，以随事务提交或回滚自动释放。");
        }

        var database = contexts.Get(module).Database;
        var acquired = new HashSet<Guid>();
        foreach (var id in ids.Distinct().OrderBy(i => LockKeys.Of(scope, i)))
        {
            var key = LockKeys.Of(scope, id);
            var taken = (await database.SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({key}) AS \"Value\"").ToListAsync(ct)).Single();
            if (taken)
            {
                acquired.Add(id);
            }
        }

        return acquired;
    }

    // 55P03（lock_not_available）：等待锁超时（超过 lock_timeout）
    private static bool IsLockTimeout(Exception exception) =>
        exception is PostgresException { SqlState: "55P03" } || exception.InnerException is PostgresException { SqlState: "55P03" };
}
