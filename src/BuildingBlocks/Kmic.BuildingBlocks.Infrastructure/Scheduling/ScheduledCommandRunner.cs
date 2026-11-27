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

internal sealed class ScheduledCommandRunner(
    ScheduledTasks tasks, ScheduledJobRuns runs, IExecutionScopeFactory scopes, FrameworkMetrics metrics, ILogger<ScheduledCommandRunner> logger)
{
    public async Task<Result<ScheduledJobOutcome>> RunAsync(
        string name, ScheduledRun run, DateTimeOffset actualFireTime, UserId? triggeredBy, string instance, CancellationToken ct)
    {
        var started = TimeProvider.System.GetTimestamp();

        // 为每次任务执行开启独立根追踪 Span（若调度器已有追踪上下文则挂载其下），并在写入执行记录前启动，确保记录中保存正确的追踪 ID
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
            // 命令管道已将业务处理器异常转换为结果；捕获至此的异常通常为框架任务的数据库异常或作用域创建失败
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
