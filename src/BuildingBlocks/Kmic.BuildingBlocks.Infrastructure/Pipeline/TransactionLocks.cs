using System.Globalization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Locking;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// <see cref="ITransactionLocks" /> 的实现：在当前命令所属模块的事务里取咨询锁
/// </summary>
internal sealed class TransactionLocks(IExecutionContext execution, CommandTransaction transaction, ModuleWriteContexts contexts) : ITransactionLocks
{
    private const string TimeoutSetting = "lock_timeout";

    public async Task<Result> AcquireSharedAsync(string scope, IReadOnlyCollection<Guid> ids, TimeSpan timeout, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentNullException.ThrowIfNull(ids);
        if (!execution.InCommand || transaction.Module is not { } module)
        {
            throw new InvalidOperationException("事务级咨询锁只能在命令里取：它要随命令的事务释放。");
        }

        if (ids.Count == 0)
        {
            return Result.Success();
        }

        var keys = ids.Distinct().Select(id => LockKeys.Of(scope, id)).Distinct().Order().ToList();
        var database = contexts.Get(module).Database;

        // 等待上限只在取锁期间生效，取完恢复原值：同一个事务里后面的语句不该被我们改过的上限影响
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

    // 55P03：lock_not_available，等锁超过了 lock_timeout
    private static bool IsLockTimeout(Exception exception) =>
        exception is PostgresException { SqlState: "55P03" } || exception.InnerException is PostgresException { SqlState: "55P03" };
}
