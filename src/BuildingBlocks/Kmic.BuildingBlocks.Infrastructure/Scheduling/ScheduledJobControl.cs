using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
using Quartz;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

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

        // Quartz 禁止同一任务并发执行，任务运行中再次触发会导致排队重复执行，因此处于忙碌状态时直接拒绝。
        // 按任务名对手动触发操作加锁排队：依次完成状态检查与触发提交后再释放锁，确保后续请求可观测到前序排队的触发记录
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

    // 查询集群共享表以覆盖跨实例场景：检查任务是否已被领取、正在执行（含即将按计划触发）或存在待执行的手动触发
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
