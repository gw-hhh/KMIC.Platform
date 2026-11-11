using System.Data.Common;
using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 查询可见性检查
/// </summary>
/// <remarks>
/// 查询上下文用的是另一个连接，查不到本命令还没提交的修改。开发和测试环境下，命令改了数据之后再查本模块就抛异常，
/// 提醒改为从仓储读取。先查后改不受影响，Contracts 查询不检查
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
                "这次查询读不到命令里还没保存的修改。要用刚改的数据就从仓储取，或者把查询放到修改之前。");
        }
    }
}
