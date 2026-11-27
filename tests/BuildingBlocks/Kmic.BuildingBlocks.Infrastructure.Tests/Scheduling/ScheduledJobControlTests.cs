using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Scheduling;

public sealed class ScheduledJobControlTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _user = TestIdentity.User("调度员").InTree(_org).InOrgUnit(_org["总部"]);
    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IScheduledJobControl Control => _harness.Services.GetRequiredService<IScheduledJobControl>();

    [Fact]
    public void Preview_ReadsTheExpressionInTheBusinessTimeZone()
    {
        var after = new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.Zero);

        var preview = Control.Preview("0 0 7 * * ?", after, 3);

        preview.IsValid.ShouldBeTrue();
        // 业务时区为东八区，每日 07:00 对应 UTC 前一日 23:00
        preview.NextFireTimes.ShouldBe([
            new DateTimeOffset(2026, 11, 10, 23, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 11, 23, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 11, 12, 23, 0, 0, TimeSpan.Zero),
        ]);
        preview.MinimumGap.ShouldBe(TimeSpan.FromDays(1));
    }

    [Theory]
    [InlineData("每天六点")]
    [InlineData("0 0 7 * *")]
    [InlineData("")]
    [InlineData("   ")]
    public void Preview_RefusesAnExpressionThatIsNotCron(string cron)
    {
        var preview = Control.Preview(cron, DateTimeOffset.UnixEpoch, 5);

        preview.IsValid.ShouldBeFalse();
        preview.Problem.ShouldNotBeNullOrWhiteSpace();
        preview.NextFireTimes.ShouldBeEmpty();
        preview.MinimumGap.ShouldBeNull();
    }

    [Theory]
    [InlineData("* * * * * ?", 1)]
    [InlineData("0/30 * * * * ?", 30)]
    [InlineData("0,10 0 * * * ?", 10)]
    [InlineData("0 * * * * ?", 60)]
    [InlineData("15 0/5 * * * ?", 300)]
    public void Preview_ReportsTheShortestGapBetweenFires(string cron, int seconds) =>
        Control.Preview(cron, DateTimeOffset.UnixEpoch, 5).MinimumGap.ShouldBe(TimeSpan.FromSeconds(seconds));

    [Fact]
    public void Preview_AnExpressionThatNeverFiresAgain_IsValidButHasNoTimes()
    {
        var preview = Control.Preview("0 0 0 1 1 ? 2000", new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.Zero), 5);

        preview.IsValid.ShouldBeTrue();
        preview.NextFireTimes.ShouldBeEmpty();
    }

    [Fact]
    public async Task List_ShowsModuleAndFrameworkTasksWithTheirTriggerState()
    {
        await ClearSettingsAsync();
        await Control.SyncAsync(Ct);

        var jobs = await Control.ListAsync(Ct);

        var daily = jobs.Single(j => string.Equals(j.Name, ProbeHarness.DailyStamp, StringComparison.Ordinal));
        daily.Module.ShouldBe("BuildingBlocks");
        daily.DisplayName.ShouldBe("每日盖章");
        daily.DefaultCron.ShouldBe("0 0 7 * * ?");
        daily.EffectiveCron.ShouldBe("0 0 7 * * ?");
        daily.IsScheduled.ShouldBeTrue();
        daily.NextFireTime.ShouldNotBeNull().UtcDateTime.TimeOfDay.ShouldBe(TimeSpan.FromHours(23));
        var framework = jobs.Where(j => j.Name.StartsWith("Framework.", StringComparison.Ordinal)).ToList();
        framework.ShouldNotBeEmpty();
        framework.ShouldAllBe(j => string.Equals(j.Module, "Framework", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Sync_FollowsTheOverridesAsTheyAreSaved()
    {
        await SetOverrideAsync(ProbeHarness.DailyStamp, enabled: false, cron: null);
        await Control.SyncAsync(Ct);
        var stopped = (await Control.ListAsync(Ct)).Single(j => string.Equals(j.Name, ProbeHarness.DailyStamp, StringComparison.Ordinal));

        await SetOverrideAsync(ProbeHarness.DailyStamp, enabled: null, cron: "0 30 6 * * ?");
        await Control.SyncAsync(Ct);
        var moved = (await Control.ListAsync(Ct)).Single(j => string.Equals(j.Name, ProbeHarness.DailyStamp, StringComparison.Ordinal));

        stopped.IsScheduled.ShouldBeFalse();
        stopped.NextFireTime.ShouldBeNull();
        stopped.DefaultCron.ShouldBe("0 0 7 * * ?");
        moved.EffectiveCron.ShouldBe("0 30 6 * * ?");
        moved.NextFireTime.ShouldNotBeNull();
        await ClearSettingsAsync();
    }

    [Fact]
    public async Task Trigger_UnknownTask_IsNotFound()
    {
        await Control.SyncAsync(Ct);

        (await Control.TriggerAsync("BuildingBlocks.Missing", _user.UserId, Ct)).ShouldBe(ScheduledJobTriggerOutcome.NotFound);
    }

    [Fact]
    public async Task Trigger_WhileTheTaskIsRunning_IsRefused_ThenWorksAgain()
    {
        await using var worker = new ProbeHarness(database.ConnectionString, services: s =>
        {
            s.AddSingleton(TimeProvider.System);
            s.AddKmicSchedulerWorker();
        });
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        worker.Hooks.OnBeforeReturn(async ct => await release.Task.WaitAsync(ct));
        await ClearSettingsAsync();
        await worker.StartHostedServicesAsync(Ct);
        try
        {
            var control = worker.Services.GetRequiredService<IScheduledJobControl>();
            (await control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct)).ShouldBe(ScheduledJobTriggerOutcome.Triggered);
            await Eventually.UntilAsync(ct => Task.FromResult(!worker.Hooks.ScheduledFires.IsEmpty), "定时任务已开始执行", TimeSpan.FromSeconds(30), Ct);

            // 任务正阻塞于命令处理中：此时再次触发“立即执行”应直接拒绝，而非排队重复执行
            (await control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct)).ShouldBe(ScheduledJobTriggerOutcome.AlreadyRunning);

            release.SetResult();
            await Eventually.UntilAsync(async ct => await RunningCountAsync(ct) == 0, "定时任务执行完成", TimeSpan.FromSeconds(30), Ct);
            await Eventually.UntilAsync(
                async ct => await control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, ct) == ScheduledJobTriggerOutcome.Triggered,
                "任务执行完成后可再次触发执行",
                TimeSpan.FromSeconds(30),
                Ct);
        }
        finally
        {
            release.TrySetResult();
            await worker.StopHostedServicesAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Trigger_WhileAnEarlierManualTriggerIsQueued_IsRefused()
    {
        await ClearSettingsAsync();
        await Control.SyncAsync(Ct);
        await RemoveManualTriggersAsync();
        try
        {
            // 当前宿主未启动调度器工作线程，首次手动触发将保留在队列中
            (await Control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct)).ShouldBe(ScheduledJobTriggerOutcome.Triggered);

            (await Control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct)).ShouldBe(ScheduledJobTriggerOutcome.AlreadyRunning);
        }
        finally
        {
            await RemoveManualTriggersAsync();
        }
    }

    [Fact]
    public async Task Trigger_ConcurrentRequests_OnlyOneIsAccepted()
    {
        await ClearSettingsAsync();
        await Control.SyncAsync(Ct);
        await RemoveManualTriggersAsync();
        await using var second = new ProbeHarness(database.ConnectionString);
        var controls = new[] { Control, second.Services.GetRequiredService<IScheduledJobControl>() };
        try
        {
            // 由两个实例并发发送共 8 次触发请求，模拟多管理员同时操作“立即执行”
            var outcomes = await Task.WhenAll(Enumerable.Range(0, 8)
                .Select(i => Task.Run(() => controls[i % 2].TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct), Ct)));

            outcomes.Count(o => o == ScheduledJobTriggerOutcome.Triggered).ShouldBe(1);
            outcomes.Count(o => o == ScheduledJobTriggerOutcome.AlreadyRunning).ShouldBe(7);
            (await ManualTriggerCountAsync()).ShouldBe(1);
        }
        finally
        {
            await RemoveManualTriggersAsync();
        }
    }

    [Fact]
    public async Task StoppingTheSchedulerServiceTwice_IsHarmless()
    {
        await using var worker = new ProbeHarness(database.ConnectionString, services: s =>
        {
            s.AddSingleton(TimeProvider.System);
            s.AddKmicSchedulerWorker();
        });
        await worker.StartHostedServicesAsync(Ct);

        await worker.StopHostedServicesAsync(CancellationToken.None);

        // 二次停机时调度器已关闭，不应再次尝试访问或停止调度器
        await Should.NotThrowAsync(async () => await worker.StopHostedServicesAsync(CancellationToken.None));
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task<long> RunningCountAsync(CancellationToken ct)
    {
        await using var command = database.DataSource.CreateCommand($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "Status" = 0""");
        return (long)(await command.ExecuteScalarAsync(ct))!;
    }

    // 手动触发会由 Quartz 生成独立的一次性触发器，而任务默认的 Cron 触发器与任务同名
    private async Task<IReadOnlyList<TriggerKey>> ManualTriggersAsync()
    {
        var scheduler = await _harness.Services.GetRequiredService<ISchedulerFactory>().GetScheduler(Ct);
        var cron = new TriggerKey(ProbeHarness.DailyStamp, CommandJob.Group);
        var triggers = await scheduler.GetTriggersOfJob(new JobKey(ProbeHarness.DailyStamp, CommandJob.Group), Ct);
        return [.. triggers.Select(t => t.Key).Where(k => !k.Equals(cron))];
    }

    private async Task<int> ManualTriggerCountAsync() => (await ManualTriggersAsync()).Count;

    private async Task RemoveManualTriggersAsync()
    {
        var scheduler = await _harness.Services.GetRequiredService<ISchedulerFactory>().GetScheduler(Ct);
        foreach (var key in await ManualTriggersAsync())
        {
            await scheduler.UnscheduleJob(key, Ct);
        }
    }

    private async Task ClearSettingsAsync()
    {
        await using var command = database.DataSource.CreateCommand($"""DELETE FROM {SchedulingTables.Settings}""");
        await command.ExecuteNonQueryAsync(Ct);
    }

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
}
