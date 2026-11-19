using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 执行一次定时任务，前后写执行记录
/// </summary>
internal sealed class ScheduledCommandRunner(
    ScheduledTasks tasks, ScheduledJobRuns runs, IExecutionScopeFactory scopes, FrameworkMetrics metrics, ILogger<ScheduledCommandRunner> logger)
{
    public async Task<Result<ScheduledJobOutcome>> RunAsync(
        string name, ScheduledRun run, DateTimeOffset actualFireTime, UserId? triggeredBy, string instance, CancellationToken ct)
    {
        var started = TimeProvider.System.GetTimestamp();

        // 每次执行一个根 span（调度器自己有追踪时挂在它下面），先于执行记录开始，记录里的追踪 ID 才是这个 span 的
        using var activity = KmicActivities.Messaging.StartActivity($"scheduled {name}", ActivityKind.Internal);
        var id = await runs.StartAsync(name, run, actualFireTime, triggeredBy, instance, Activity.Current?.TraceId.ToString(), ct);
        try
        {
            var result = await ExecuteAsync(name, run, ct);
            await FinishAsync(id, name, result);
            metrics.ScheduledJobFinished(name, result.IsSuccess, TimeProvider.System.GetElapsedTime(started));
            return result;
        }
        catch (Exception exception)
        {
            // 命令管道已经把处理器里的异常换成了结果，能到这里的是框架任务的数据库错误、建作用域失败这类问题
            metrics.ScheduledJobFinished(name, succeeded: false, TimeProvider.System.GetElapsedTime(started));
            SchedulingLog.RunThrew(logger, exception, name);
            await runs.FinishAsync(id, ScheduledRunStatus.Failed, null, exception.GetType().Name, exception.Message, CancellationToken.None);
            throw;
        }
    }

    private async Task<Result<ScheduledJobOutcome>> ExecuteAsync(string name, ScheduledRun run, CancellationToken ct)
    {
        if (tasks.FindFrameworkTask(name) is { } task)
        {
            return await task.ExecuteAsync(ct);
        }

        var job = tasks.FindModuleJob(name) ?? throw new InvalidOperationException($"本实例没有声明定时任务 {name}。");
        await using var scope = scopes.CreateScope(CallOrigin.ScheduledJob, CurrentUser.System());
        return await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(job.Create(run), ct);
    }

    private async Task FinishAsync(Guid id, string name, Result<ScheduledJobOutcome> result)
    {
        if (result.IsSuccess)
        {
            await runs.FinishAsync(id, ScheduledRunStatus.Succeeded, result.Value, null, null, CancellationToken.None);
            return;
        }

        var unknown = string.Equals(result.Error.Code, CommonErrors.ResultUnknown.Code, StringComparison.Ordinal);
        if (unknown)
        {
            SchedulingLog.RunOutcomeUnknown(logger, name);
        }
        else
        {
            SchedulingLog.RunFailed(logger, name, result.Error.Code);
        }

        await runs.FinishAsync(
            id, unknown ? ScheduledRunStatus.Unknown : ScheduledRunStatus.Failed, null, result.Error.Code, result.Error.Message, CancellationToken.None);
    }
}
