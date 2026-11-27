using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

internal sealed class ScheduleSynchronizer(
    ISchedulerFactory schedulers,
    ScheduledTasks tasks,
    ScheduledJobSettings settings,
    IBusinessCalendar calendar,
    DataSources dataSources,
    ILogger<ScheduleSynchronizer> logger)
{
    // 在作业元数据中记录最后同步的构建版本，防止旧版本实例回退覆盖新版本的同步配置
    private const string BuildKey = "kmic.build";

    public async Task SyncAsync(string build, CancellationToken ct)
    {
        // 多实例并发启动时通过分布式锁串行同步，后续实例若检测到配置已一致则跳过更新
        await using var connection = await dataSources.Transactions(CallOrigin.Startup).OpenConnectionAsync(ct);
        await using (var lockCommand = connection.CreateCommand())
        {
            lockCommand.CommandText = "SELECT pg_advisory_lock(hashtext('Framework.SyncSchedules'))";
            await lockCommand.ExecuteNonQueryAsync(ct);
        }

        try
        {
            var scheduler = await schedulers.GetScheduler(ct);
            var overrides = await settings.ReadAsync(ct);
            var changed = 0;
            foreach (var job in tasks.All)
            {
                if (await SyncOneAsync(scheduler, job, overrides.GetValueOrDefault(job.Name), build, ct))
                {
                    changed++;
                }
            }

            var removed = await RemoveUndeclaredAsync(scheduler, build, ct);
            SchedulingLog.Synced(logger, changed, removed);
        }
        finally
        {
            await using var unlock = connection.CreateCommand();
            unlock.CommandText = "SELECT pg_advisory_unlock(hashtext('Framework.SyncSchedules'))";
            await unlock.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    // 立即触发一次任务（可在非执行节点调用，交由集群中运行调度器的 Worker 实例执行）
    public async Task<bool> TriggerAsync(string name, UserId? by, CancellationToken ct)
    {
        var scheduler = await schedulers.GetScheduler(ct);
        var key = new JobKey(name, CommandJob.Group);
        if (!await scheduler.Exists(key, ct))
        {
            return false;
        }

        await scheduler.TriggerJob(key, new JobDataMap { [CommandJob.TriggeredByKey] = by?.Value.ToString() ?? "" }, ct);
        return true;
    }

    internal static ITrigger BuildTrigger(ScheduleEntry job, string cron, TimeZoneInfo zone) =>
        TriggerBuilder.Create()
            .WithIdentity(job.Name, CommandJob.Group)
            .ForJob(job.Name, CommandJob.Group)
            .WithCronSchedule(cron, b => b.InTimeZone(zone).WithMisfireInstruction(Instruction(job.Misfire)))
            .Build();

    private static CronTriggerMisfireInstruction Instruction(Misfire misfire) =>
        misfire == Misfire.Skip ? CronTriggerMisfireInstruction.DoNothing : CronTriggerMisfireInstruction.FireAndProceed;

    private static bool IsNewer(string? recorded, string build) =>
        ParseBuild(recorded) is { } theirs && ParseBuild(build) is { } ours && theirs > ours;

    // 提取版本号中 '+' 或 '-' 之前的构建号；无法解析的版本（如本地开发的 unknown）不参与版本新旧比较
    private static Version? ParseBuild(string? build)
    {
        if (build is null)
        {
            return null;
        }

        return Version.TryParse(build.Split('+', '-')[0], out var version) ? version : null;
    }

    private async Task<bool> SyncOneAsync(IScheduler scheduler, ScheduleEntry job, ScheduledJobSetting? setting, string build, CancellationToken ct)
    {
        var jobKey = new JobKey(job.Name, CommandJob.Group);
        var existing = await scheduler.GetJobDetail(jobKey, ct);
        var recorded = existing?.JobDataMap.TryGetValue(BuildKey, out var value) == true ? value as string : null;
        if (IsNewer(recorded, build))
        {
            SchedulingLog.NewerBuild(logger, job.Name, recorded!);
            return false;
        }

        var changed = false;
        if (existing is null || !string.Equals(recorded, build, StringComparison.Ordinal) || !string.Equals(existing.Description, job.DisplayName, StringComparison.Ordinal))
        {
            var detail = JobBuilder.Create<CommandJob>()
                .WithIdentity(jobKey)
                .WithDescription(job.DisplayName)
                .StoreDurably()
                .UsingJobData(BuildKey, build)
                .Build();
            await scheduler.AddJob(detail, AddJobOptions.Replacing, ct);
            changed = true;
        }

        var triggerKey = new TriggerKey(job.Name, CommandJob.Group);
        var current = await scheduler.GetTrigger(triggerKey, ct);
        if (setting?.Enabled == false)
        {
            return current is not null ? await scheduler.UnscheduleJob(triggerKey, ct) : changed;
        }

        var cron = setting?.Cron ?? job.Cron;
        if (!CronExpression.TryParse(cron, out _))
        {
            SchedulingLog.InvalidOverride(logger, job.Name, cron);
            cron = job.Cron;
        }

        if (current is ICronTrigger same
            && string.Equals(same.CronExpressionString, cron, StringComparison.Ordinal)
            && string.Equals(same.TimeZone.Id, calendar.Zone.Id, StringComparison.Ordinal)
            && same.MisfireInstruction == Instruction(job.Misfire))
        {
            return changed;
        }

        await scheduler.ScheduleJob(BuildTrigger(job, cron, calendar.Zone), ScheduleJobOptions.Replacing, ct);
        return true;
    }

    private async Task<int> RemoveUndeclaredAsync(IScheduler scheduler, string build, CancellationToken ct)
    {
        var removed = 0;
        foreach (var key in await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(CommandJob.Group), ct))
        {
            if (tasks.Contains(key.Name))
            {
                continue;
            }

            // 保留新版本新增的任务，避免旧版本实例因无法识别而误删
            var detail = await scheduler.GetJobDetail(key, ct);
            if (detail is not null && detail.JobDataMap.TryGetValue(BuildKey, out var recorded) && IsNewer(recorded as string, build))
            {
                continue;
            }

            // 保留历史运维覆盖配置，以便后续版本回滚恢复该任务时原有运维调整仍有效
            await scheduler.DeleteJob(key, ct);
            removed++;
        }

        return removed;
    }
}
