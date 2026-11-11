using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Scheduling;

/// <summary>
/// 同步只改 Quartz 里的内容，不需要启动调度器
/// </summary>
public sealed class ScheduleSyncTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Build = "1.0.0";

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Sync_DurableJobWithCronTriggerInBusinessTimeZone()
    {
        await SyncAsync(_harness);

        var job = (await JobAsync(ProbeHarness.DailyStamp)).ShouldNotBeNull();
        job.Description.ShouldBe("每日盖章");
        job.Durable.ShouldBeTrue();
        var trigger = (await TriggerAsync(ProbeHarness.DailyStamp)).ShouldBeAssignableTo<ICronTrigger>()!;
        trigger.CronExpressionString.ShouldBe("0 0 7 * * ?");
        trigger.TimeZone.Id.ShouldBe("Asia/Shanghai");
        trigger.MisfireInstruction.ShouldBe(CronTriggerMisfireInstruction.FireAndProceed);

        // 业务时间早上 7 点就是 UTC 前一天 23 点
        trigger.GetFireTimeAfter(trigger.StartTimeUtc).ShouldNotBeNull().UtcDateTime.TimeOfDay.ShouldBe(TimeSpan.FromHours(23));
    }

    [Fact]
    public async Task Override_DisableChangeTimeThenRestoreDefault()
    {
        await SetOverrideAsync(ProbeHarness.DailyStamp, enabled: false, cron: null);
        await SyncAsync(_harness);
        (await TriggerAsync(ProbeHarness.DailyStamp)).ShouldBeNull();
        (await JobAsync(ProbeHarness.DailyStamp)).ShouldNotBeNull();

        await SetOverrideAsync(ProbeHarness.DailyStamp, enabled: null, cron: "0 30 6 * * ?");
        await SyncAsync(_harness);
        (await TriggerAsync(ProbeHarness.DailyStamp)).ShouldBeAssignableTo<ICronTrigger>()!.CronExpressionString.ShouldBe("0 30 6 * * ?");

        await SetOverrideAsync(ProbeHarness.DailyStamp, enabled: null, cron: "每天六点");
        await SyncAsync(_harness);
        (await TriggerAsync(ProbeHarness.DailyStamp)).ShouldBeAssignableTo<ICronTrigger>()!.CronExpressionString.ShouldBe("0 0 7 * * ?");

        await ExecuteAsync($"""DELETE FROM {SchedulingTables.Settings}""");
        await SyncAsync(_harness);
        (await TriggerAsync(ProbeHarness.DailyStamp)).ShouldBeAssignableTo<ICronTrigger>()!.CronExpressionString.ShouldBe("0 0 7 * * ?");
    }

    [Fact]
    public async Task RemovedFromCode_JobDeleted_OverrideKept()
    {
        const string Retired = "BuildingBlocks.Retired";
        await using (var before = new ProbeHarness(database.ConnectionString, services: s => s.AddKmicScheduling(
            "BuildingBlocks", x => x.Schedule(Retired, "0 0 1 * * ?", run => new StampTicketsCommand(run.ScheduledFireTime)))))
        {
            await SyncAsync(before);
        }

        await SetOverrideAsync(Retired, enabled: false, cron: null);
        (await JobAsync(Retired)).ShouldNotBeNull();

        await SyncAsync(_harness);

        (await JobAsync(Retired)).ShouldBeNull();
        (await ScalarAsync($"""SELECT count(*) FROM {SchedulingTables.Settings} WHERE "JobName" = '{Retired}'""")).ShouldBe(1);
        await ExecuteAsync($"""DELETE FROM {SchedulingTables.Settings}""");
    }

    [Fact]
    public async Task SyncAgain_NothingChanged_NextFireTimeKept()
    {
        await SyncAsync(_harness);
        var first = await TriggerAsync(ProbeHarness.DailyStamp);

        await SyncAsync(_harness);

        var second = await TriggerAsync(ProbeHarness.DailyStamp);
        second.ShouldNotBeNull().NextFireTimeUtc.ShouldBe(first.ShouldNotBeNull().NextFireTimeUtc);
        second.StartTimeUtc.ShouldBe(first.StartTimeUtc);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static Task SyncAsync(ProbeHarness harness) => harness.Services.GetRequiredService<ScheduleSynchronizer>().SyncAsync(Build, Ct);

    private async Task<IJobDetail?> JobAsync(string name) =>
        await (await SchedulerAsync()).GetJobDetail(new JobKey(name, CommandJob.Group), Ct);

    private async Task<ITrigger?> TriggerAsync(string name) =>
        await (await SchedulerAsync()).GetTrigger(new TriggerKey(name, CommandJob.Group), Ct);

    private ValueTask<IScheduler> SchedulerAsync() => _harness.Services.GetRequiredService<ISchedulerFactory>().GetScheduler(Ct);

    private async Task SetOverrideAsync(string job, bool? enabled, string? cron)
    {
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO {SchedulingTables.Settings} ("JobName", "Enabled", "Cron") VALUES (@job, @enabled, @cron)
            ON CONFLICT ("JobName") DO UPDATE SET "Enabled" = excluded."Enabled", "Cron" = excluded."Cron"
            """);
        command.Parameters.AddWithValue("job", job);
        command.Parameters.AddWithValue("enabled", (object?)enabled ?? DBNull.Value);
        command.Parameters.AddWithValue("cron", (object?)cron ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<long> ScalarAsync(string sql)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }
}
