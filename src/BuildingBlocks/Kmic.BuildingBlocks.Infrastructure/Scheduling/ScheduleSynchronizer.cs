using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 把代码里的默认值和运维改过的覆盖值同步到 Quartz
/// </summary>
/// <remarks>
/// 启动时同步全部，运维改了开关或时间后再同步一次。每个任务在 Quartz 里是一个持久的作业加一个 Cron 触发器，
/// 停用就是去掉触发器，作业保留，仍可手动执行
/// </remarks>
internal sealed class ScheduleSynchronizer(
    ISchedulerFactory schedulers,
    ScheduledJobCatalog catalog,
    ScheduledJobSettings settings,
    IBusinessCalendar calendar,
    DataSources dataSources,
    ILogger<ScheduleSynchronizer> logger)
{
    // 作业上记下最后同步它的构建，旧版本的实例不覆盖新版本写下的
    private const string BuildKey = "kmic.build";

    public async Task SyncAsync(string build, CancellationToken ct)
    {
        // 多个实例同时启动时排队做，后来的看到已经一致就什么都不改
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
            foreach (var job in catalog.All)
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

    /// <summary>
    /// 马上执行一次，可以在只调度不执行的实例上调用，由正在运行调度器的实例执行
    /// </summary>
    /// <returns>Quartz 里没有这个任务时返回 false</returns>
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

    /// <summary>
    /// 按默认值和覆盖值得到的 Cron 触发器
    /// </summary>
    internal static ITrigger BuildTrigger(ScheduledJobDefinition job, string cron, TimeZoneInfo zone) =>
        TriggerBuilder.Create()
            .WithIdentity(job.Name, CommandJob.Group)
            .ForJob(job.Name, CommandJob.Group)
            .WithCronSchedule(cron, b => b.InTimeZone(zone).WithMisfireInstruction(Instruction(job.Misfire)))
            .Build();

    private static CronTriggerMisfireInstruction Instruction(Misfire misfire) =>
        misfire == Misfire.Skip ? CronTriggerMisfireInstruction.DoNothing : CronTriggerMisfireInstruction.FireAndProceed;

    private static bool IsNewer(string? recorded, string build) =>
        ParseBuild(recorded) is { } theirs && ParseBuild(build) is { } ours && theirs > ours;

    // 构建号取版本号中“+”“-”之前的部分；读不出来的（如本机开发的 unknown）不参与比较
    private static Version? ParseBuild(string? build)
    {
        if (build is null)
        {
            return null;
        }

        return Version.TryParse(build.Split('+', '-')[0], out var version) ? version : null;
    }

    private async Task<bool> SyncOneAsync(IScheduler scheduler, ScheduledJobDefinition job, ScheduledJobSetting? setting, string build, CancellationToken ct)
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
            if (catalog.Find(key.Name) is not null)
            {
                continue;
            }

            // 新版本加的任务，旧版本的实例不认识，也不删
            var detail = await scheduler.GetJobDetail(key, ct);
            if (detail is not null && detail.JobDataMap.TryGetValue(BuildKey, out var recorded) && IsNewer(recorded as string, build))
            {
                continue;
            }

            // 覆盖值保留：回滚到还有这个任务的版本时，运维改过的设置还在
            await scheduler.DeleteJob(key, ct);
            removed++;
        }

        return removed;
    }
}
