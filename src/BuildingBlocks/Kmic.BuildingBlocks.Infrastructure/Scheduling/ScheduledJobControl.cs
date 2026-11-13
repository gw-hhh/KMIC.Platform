using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// <see cref="IScheduledJobControl" /> 的实现：包装同步器和调度器，给 Operations 的管理命令用
/// </summary>
internal sealed class ScheduledJobControl(
    ISchedulerFactory schedulers,
    ScheduledTasks tasks,
    ScheduleSynchronizer synchronizer,
    IBusinessCalendar calendar,
    DataSources dataSources) : IScheduledJobControl
{
    public bool IsDeclared(string name) => tasks.Contains(name);

    public async Task<IReadOnlyList<ScheduledJobInfo>> ListAsync(CancellationToken ct)
    {
        var scheduler = await schedulers.GetScheduler(ct);
        var list = new List<ScheduledJobInfo>(tasks.All.Count);
        foreach (var entry in tasks.All)
        {
            var trigger = await scheduler.GetTrigger(new TriggerKey(entry.Name, CommandJob.Group), ct) as ICronTrigger;
            list.Add(new ScheduledJobInfo(
                entry.Name, entry.Module, entry.DisplayName, entry.Cron, entry.Misfire, trigger?.CronExpressionString, trigger?.NextFireTimeUtc));
        }

        return list;
    }

    public Task SyncAsync(CancellationToken ct) => synchronizer.SyncAsync(BuildInfo.Version, ct);

    public async Task<ScheduledJobTriggerOutcome> TriggerAsync(string name, UserId by, CancellationToken ct)
    {
        if (!tasks.Contains(name))
        {
            return ScheduledJobTriggerOutcome.NotFound;
        }

        // Quartz 不让同一个任务并发执行，这时再触发只会排队等着；管理员点的“立即执行”要么马上开始，要么告诉他正在运行。
        // 本实例上的执行不够：任务可能在别的实例上跑，所以看集群共享的“正在触发”表
        if (await IsExecutingAsync(name, ct))
        {
            return ScheduledJobTriggerOutcome.AlreadyRunning;
        }

        return await synchronizer.TriggerAsync(name, by, ct) ? ScheduledJobTriggerOutcome.Triggered : ScheduledJobTriggerOutcome.NotFound;
    }

    public CronPreview Preview(string cron, DateTimeOffset after, int count)
    {
        if (string.IsNullOrWhiteSpace(cron))
        {
            return new CronPreview("执行时间不能为空", []);
        }

        CronExpression expression;
        try
        {
            expression = CronExpression.Parse(cron).WithTimeZone(calendar.Zone);
        }
        catch (FormatException exception)
        {
            return new CronPreview(exception.Message, []);
        }

        var times = new List<DateTimeOffset>(count);
        var cursor = after;
        while (times.Count < count && expression.GetNextValidTimeAfter(cursor) is { } next)
        {
            times.Add(next);
            cursor = next;
        }

        return new CronPreview(null, times);
    }

    private async Task<bool> IsExecutingAsync(string name, CancellationToken ct)
    {
        await using var command = dataSources.Transactions(CallOrigin.ScheduledJob).CreateCommand(
            $"""
            SELECT EXISTS (
                SELECT 1 FROM {SchedulingTables.QuartzPrefix}fired_triggers
                WHERE job_name = @name AND job_group = @group AND state = 'EXECUTING')
            """);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("group", CommandJob.Group);
        return (bool)(await command.ExecuteScalarAsync(ct))!;
    }
}
