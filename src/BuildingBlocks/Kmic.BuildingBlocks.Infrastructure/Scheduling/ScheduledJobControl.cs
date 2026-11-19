using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
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

        // Quartz 不让同一个任务并发执行，任务忙时再触发只会排在后面再跑一遍，所以忙时直接拒绝。
        // 同一任务的手动触发按任务名排队：持锁检查、提交触发，提交完才放锁，后来的请求能看到前一个排队的触发
        await using var connection = await dataSources.Transactions(CallOrigin.ScheduledJob).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using (var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext('Kmic.ScheduledJobTrigger:' || @name))", connection, transaction))
        {
            command.Parameters.AddWithValue("name", name);
            await command.ExecuteNonQueryAsync(ct);
        }

        if (await IsBusyAsync(connection, transaction, name, ct))
        {
            return ScheduledJobTriggerOutcome.AlreadyRunning;
        }

        var triggered = await synchronizer.TriggerAsync(name, by, ct);
        await transaction.CommitAsync(ct);
        return triggered ? ScheduledJobTriggerOutcome.Triggered : ScheduledJobTriggerOutcome.NotFound;
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

    // 看集群共享的表，任务可能在别的实例上：已被领取或正在执行（含即将按计划执行的），或者还有没执行的手动触发
    private static async Task<bool> IsBusyAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string name, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            SELECT EXISTS (
                       SELECT 1 FROM {SchedulingTables.QuartzPrefix}fired_triggers
                       WHERE job_name = @name AND job_group = @group)
                OR EXISTS (
                       SELECT 1 FROM {SchedulingTables.QuartzPrefix}triggers
                       WHERE job_name = @name AND job_group = @group AND NOT (trigger_name = @name AND trigger_group = @group))
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("group", CommandJob.Group);
        return (bool)(await command.ExecuteScalarAsync(ct))!;
    }
}
