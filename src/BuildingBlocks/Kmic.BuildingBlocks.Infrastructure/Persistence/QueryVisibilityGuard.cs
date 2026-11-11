using System.Data.Common;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 开发与测试环境的守卫：命令已经修改了聚合、尚未保存时，再发送本模块的查询即抛异常
/// </summary>
/// <remarks>
/// 查询上下文用读池的另一个连接，看不到本命令尚未提交的修改；命令中用于判断与修改的数据要经仓储读取（05-03 第 2.1 节）。
/// 先查询、后修改不受影响；其他模块的 Contracts 查询不检查
/// </remarks>
internal sealed class QueryVisibilityGuard(ExecutionScope execution, ModuleWriteContexts writeContexts) : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Check(eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        Check(eventData);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData);
        return ValueTask.FromResult(result);
    }

    private void Check(CommandEventData eventData)
    {
        if (!execution.InCommand || eventData.Context is not ModuleQueryDbContext query)
        {
            return;
        }

        var module = ModuleNames.Of(query.GetType());
        if (writeContexts.Get(module).ChangeTracker.HasChanges())
        {
            throw new InvalidOperationException(
                $"命令已修改 {query.Module} 模块的数据、尚未保存，此时的查询读不到这些修改：命令中用于判断的数据要经仓储读取，查询放到修改之前。");
        }
    }
}
