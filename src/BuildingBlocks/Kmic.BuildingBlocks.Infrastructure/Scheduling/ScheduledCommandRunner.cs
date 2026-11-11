using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 以系统身份发送定时任务的命令，前后写执行记录
/// </summary>
internal sealed class ScheduledCommandRunner(ScheduledJobRuns runs, IExecutionScopeFactory scopes, ILogger<ScheduledCommandRunner> logger)
{
    /// <param name="job">定时任务</param>
    /// <param name="run">这次触发</param>
    /// <param name="actualFireTime">实际触发时刻</param>
    /// <param name="triggeredBy">手动执行的人</param>
    /// <param name="instance">执行它的调度器实例</param>
    /// <param name="ct">停机时取消</param>
    public async Task<Result<ScheduledJobOutcome>> RunAsync(
        ScheduledJobDefinition job, ScheduledRun run, DateTimeOffset actualFireTime, UserId? triggeredBy, string instance, CancellationToken ct)
    {
        var id = await runs.StartAsync(job.Name, run, actualFireTime, triggeredBy, instance, Activity.Current?.TraceId.ToString(), ct);
        try
        {
            Result<ScheduledJobOutcome> result;
            await using (var scope = scopes.CreateScope(CallOrigin.ScheduledJob, CurrentUser.System()))
            {
                result = await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(job.Create(run), ct);
            }

            await FinishAsync(id, job, result);
            return result;
        }
        catch (Exception exception)
        {
            // 命令管道已经把处理器里的异常换成了结果，能到这里的是构造命令、建作用域这类框架层面的问题
            SchedulingLog.RunThrew(logger, exception, job.Name);
            await runs.FinishAsync(id, ScheduledRunStatus.Failed, null, exception.GetType().Name, exception.Message, CancellationToken.None);
            throw;
        }
    }

    private async Task FinishAsync(Guid id, ScheduledJobDefinition job, Result<ScheduledJobOutcome> result)
    {
        if (result.IsSuccess)
        {
            await runs.FinishAsync(id, ScheduledRunStatus.Succeeded, result.Value, null, null, CancellationToken.None);
            return;
        }

        var unknown = string.Equals(result.Error.Code, CommonErrors.ResultUnknown.Code, StringComparison.Ordinal);
        if (unknown)
        {
            SchedulingLog.RunOutcomeUnknown(logger, job.Name);
        }
        else
        {
            SchedulingLog.RunFailed(logger, job.Name, result.Error.Code);
        }

        await runs.FinishAsync(
            id, unknown ? ScheduledRunStatus.Unknown : ScheduledRunStatus.Failed, null, result.Error.Code, result.Error.Message, CancellationToken.None);
    }
}
