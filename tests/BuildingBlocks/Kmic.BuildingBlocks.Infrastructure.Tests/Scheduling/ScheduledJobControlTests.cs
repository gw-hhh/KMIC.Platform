using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Scheduling;

/// <summary>
/// 给 Operations 用的运行控制：看状态、预览 Cron、同步覆盖值、马上执行（含正在运行时的拒绝）
/// </summary>
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
        // 业务时区是东八区，早上 7 点就是前一天 23 点（UTC）
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
            await Eventually.UntilAsync(ct => Task.FromResult(!worker.Hooks.ScheduledFires.IsEmpty), "任务开始执行", TimeSpan.FromSeconds(30), Ct);

            // 任务停在命令里：这时再点“立即执行”要被拒绝，而不是排在后面再跑一遍
            (await control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, Ct)).ShouldBe(ScheduledJobTriggerOutcome.AlreadyRunning);

            release.SetResult();
            await Eventually.UntilAsync(async ct => await RunningCountAsync(ct) == 0, "任务执行完", TimeSpan.FromSeconds(30), Ct);
            await Eventually.UntilAsync(
                async ct => await control.TriggerAsync(ProbeHarness.DailyStamp, _user.UserId, ct) == ScheduledJobTriggerOutcome.Triggered,
                "执行完之后可以再次执行",
                TimeSpan.FromSeconds(30),
                Ct);
        }
        finally
        {
            release.TrySetResult();
            await worker.StopHostedServicesAsync(CancellationToken.None);
        }
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task<long> RunningCountAsync(CancellationToken ct)
    {
        await using var command = database.DataSource.CreateCommand($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "Status" = 0""");
        return (long)(await command.ExecuteScalarAsync(ct))!;
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
