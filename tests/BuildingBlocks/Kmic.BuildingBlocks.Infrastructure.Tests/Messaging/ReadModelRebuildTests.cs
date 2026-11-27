using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Jobs;
using Kmic.Echo.Probe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

public sealed class ReadModelRebuildTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const string Module = "Echo";

    private readonly ProbeHarness _harness = new(database.ConnectionString, messaging: o => o.Rebuild.PageSize = 2, withRebuild: true);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private FakeTicketSource Source => _harness.TicketSource;

    private JobExecutor Executor => _harness.Services.GetRequiredService<JobExecutor>();

    public async ValueTask InitializeAsync()
    {
        foreach (var sql in new[]
        {
            """DELETE FROM "Echo"."ReadModelRebuildRun" """,
            """DELETE FROM "Echo"."BackgroundJob" """,
            """DELETE FROM "Echo"."TicketSummary" """,
            $"""DELETE FROM "Operations"."MessageRoute" WHERE "Subscription" = '{ProbeHarness.MirrorSubscription}'""",
        })
        {
            await using var command = database.DataSource.CreateCommand(sql);
            await command.ExecuteNonQueryAsync(Ct);
        }
    }

    [Fact]
    public async Task EmptyReadModel_EverySnapshotCopied()
    {
        for (var n = 1; n <= 5; n++)
        {
            Source.Set($"单据{n}", version: n, Id(n));
        }

        var runId = await QueueAsync();
        await _harness.Services.RunJobsAsync(Ct);

        var rows = await SummariesAsync();
        rows.Keys.ShouldBe([Id(1), Id(2), Id(3), Id(4), Id(5)]);
        rows[Id(3)].Title.ShouldBe("单据3");
        rows[Id(3)].SourceVersion.ShouldBe(3);
        rows.Values.ShouldAllBe(r => r.RebuildRunId == runId && !r.IsTerminal);
        Source.Pages.ShouldBe(3);

        var run = await RunAsync(runId);
        (run.Status, run.ScannedCount, run.MissingCount).ShouldBe((RebuildRunStatus.Completed, 5L, 0L));
        run.FinishedAt.ShouldNotBeNull();
        (await SummaryOfJobAsync(run.JobId)).GetProperty("scanned").GetInt64().ShouldBe(5);
    }

    [Fact]
    public async Task ExistingRows_ContentRepairedButVersionNeverGoesBack()
    {
        await SeedAsync(Id(1), "被改坏的", version: 3);
        await SeedAsync(Id(2), "本地更新", version: 9);
        Source.Set("正确的", version: 3, Id(1));
        Source.Set("较旧的", version: 5, Id(2));
        Source.Set("新增的", version: 1, Id(3));

        var runId = await QueueAsync();
        await _harness.Services.RunJobsAsync(Ct);

        var rows = await SummariesAsync();
        rows[Id(1)].Title.ShouldBe("正确的");
        rows[Id(2)].Title.ShouldBe("本地更新");
        rows[Id(2)].SourceVersion.ShouldBe(9);
        rows[Id(3)].Title.ShouldBe("新增的");
        rows.Values.ShouldAllBe(r => r.RebuildRunId == runId);
    }

    [Fact]
    public async Task RowGoneFromSource_ConfirmedMissingAndMarkedTerminal()
    {
        await SeedAsync(Id(1), "还在", version: 1);
        await SeedAsync(Id(2), "已被物理删除", version: 4);
        Source.Set("还在", version: 1, Id(1));

        var runId = await QueueAsync();
        await _harness.Services.RunJobsAsync(Ct);

        var rows = await SummariesAsync();
        rows[Id(1)].IsTerminal.ShouldBeFalse();
        rows[Id(2)].IsTerminal.ShouldBeTrue();
        rows[Id(2)].TerminalReason.ShouldBe(ReadModelTerminalReason.ConfirmedMissing);
        rows[Id(2)].TerminalAt.ShouldNotBeNull();
        rows[Id(2)].SourceVersion.ShouldBe(4);
        var run = await RunAsync(runId);
        (run.Status, run.MissingCount).ShouldBe((RebuildRunStatus.Completed, 1L));
        (await SummaryOfJobAsync(run.JobId)).GetProperty("markedGone").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task ManyMissingUnderLimit_AllMarkedAcrossCandidatePages()
    {
        for (var n = 1; n <= 6; n++)
        {
            await SeedAsync(Id(n), $"单据{n}", version: 1);
        }

        Source.Set("单据1", version: 1, Id(1));
        Source.Set("单据2", version: 1, Id(2));

        var runId = await QueueAsync();
        await _harness.Services.RunJobsAsync(Ct);

        var rows = await SummariesAsync();
        rows.Values.Where(r => r.IsTerminal).Select(r => r.Id).ShouldBe([Id(3), Id(4), Id(5), Id(6)], ignoreOrder: true);
        (await RunAsync(runId)).MissingCount.ShouldBe(4);
    }

    [Fact]
    public async Task RowCreatedAfterScanPassed_ConfirmedBySourceNotMarkedGone()
    {
        Source.Set("一", version: 1, Id(1));
        Source.Set("二", version: 1, Id(2));
        Source.Set("三", version: 1, Id(3));

        // 扫描读取第二页前，源端新增了排序靠前的单据且已通过实时事件写入读模型，后续扫描不会再次覆盖该位置
        Source.BeforePage = async (page, _) =>
        {
            if (page == 2)
            {
                Source.Set("扫描之后新建", version: 1, Id(0));
                await SeedAsync(Id(0), "扫描之后新建", version: 1);
            }
        };

        var runId = await QueueAsync();
        await _harness.Services.RunJobsAsync(Ct);

        var rows = await SummariesAsync();
        rows[Id(0)].IsTerminal.ShouldBeFalse();
        rows[Id(0)].RebuildRunId.ShouldBe(runId);
        (await RunAsync(runId)).MissingCount.ShouldBe(0);
    }

    [Fact]
    public async Task TerminalRowSourceStillReturns_LeftUnchanged()
    {
        await SeedAsync(Id(1), "占位", version: 5);
        await WithWriterAsync(w => w.PurgeAsync(Row(Id(1), "", version: 6, ReferenceStatus.Deleted), Ct));
        Source.Set("源端又有了", version: 7, Id(1));

        var runId = await QueueAsync();
        await _harness.Services.RunJobsAsync(Ct);

        var row = (await SummariesAsync())[Id(1)];
        row.IsTerminal.ShouldBeTrue();
        row.TerminalReason.ShouldBe(ReadModelTerminalReason.PurgeEvent);
        row.SourceVersion.ShouldBe(6);
        row.Title.ShouldBe("占位");
        (await RunAsync(runId)).Status.ShouldBe(RebuildRunStatus.Completed);
    }

    [Fact]
    public async Task TooManyMissing_PausedWithoutMarkingAnything()
    {
        await using var harness = new ProbeHarness(
            database.ConnectionString,
            messaging: o =>
            {
                o.Rebuild.PageSize = 2;
                o.Rebuild.MaxMissingCount = 1;
                o.Rebuild.MaxMissingRatio = 0;
            },
            withRebuild: true);
        await SeedAsync(Id(1), "还在", version: 1);
        await SeedAsync(Id(2), "没了一", version: 1);
        await SeedAsync(Id(3), "没了二", version: 1);
        await SeedAsync(Id(4), "没了三", version: 1);
        harness.TicketSource.Set("还在", version: 1, Id(1));

        var runId = await QueueAsync(harness);
        await harness.Services.RunJobsAsync(Ct);

        (await SummariesAsync()).Values.ShouldAllBe(r => !r.IsTerminal);
        var run = await RunAsync(runId);
        run.Status.ShouldBe(RebuildRunStatus.AwaitingConfirmation);
        run.MissingCount.ShouldBeGreaterThan(1);
        run.FinishedAt.ShouldBeNull();
        (await SummaryOfJobAsync(run.JobId)).GetProperty("awaitingConfirmation").GetBoolean().ShouldBeTrue();
        (await JobStatusAsync(run.JobId)).ShouldBe(JobStatus.Succeeded);

        // 处于待确认暂停状态的轮次仍占用当前重建槽位，不会重复排队新轮次
        var again = await harness.Services.GetRequiredService<RebuildRunStore>()
            .QueueAsync(CallOrigin.Startup, Module, ProbeHarness.MirrorSubscription, nameof(TicketSummary), Ct);
        again.ShouldBeNull();
    }

    // 构造因缺失行数超限而暂停的重建轮次：本地存在 3 条源端已缺失的记录，缺失上限设为 1
    private static ProbeHarness PausingHarness(string connectionString) =>
        new(
            connectionString,
            messaging: o =>
            {
                o.Rebuild.PageSize = 2;
                o.Rebuild.MaxMissingCount = 1;
                o.Rebuild.MaxMissingRatio = 0;
            },
            withRebuild: true);

    private async Task<Guid> PausedRunAsync(ProbeHarness harness)
    {
        await SeedAsync(Id(1), "还在", version: 1);
        await SeedAsync(Id(2), "没了一", version: 1);
        await SeedAsync(Id(3), "没了二", version: 1);
        await SeedAsync(Id(4), "没了三", version: 1);
        harness.TicketSource.Set("还在", version: 1, Id(1));
        await RunStartupTasksAsync(harness);
        await harness.Services.RunJobsAsync(Ct);
        var run = (await RunsAsync()).Single();
        run.Status.ShouldBe(RebuildRunStatus.AwaitingConfirmation);
        return run.Id;
    }

    [Fact]
    public async Task Admin_Confirm_MarksTheMissingRowsAndCompletesTheRun()
    {
        await using var harness = PausingHarness(database.ConnectionString);
        var runId = await PausedRunAsync(harness);
        var admin = harness.Services.GetRequiredService<IReadModelRebuildAdmin>();

        (await admin.ConfirmAsync(ProbeHarness.MirrorSubscription, Ct)).ShouldBe(RebuildAdminOutcome.Done);
        (await RunAsync(runId)).Status.ShouldBe(RebuildRunStatus.Active, "管理员确认后轮次应恢复为进行中状态，并由后续作业完成收尾标记");
        await harness.Services.RunJobsAsync(Ct);

        var run = await RunAsync(runId);
        run.Status.ShouldBe(RebuildRunStatus.Completed);
        run.MissingCount.ShouldBe(3);
        var rows = await SummariesAsync();
        rows[Id(1)].IsTerminal.ShouldBeFalse();
        new[] { Id(2), Id(3), Id(4) }.ShouldAllBe(id => rows[id].IsTerminal);
        (await NeedsInitializationAsync()).ShouldBeFalse();
        (await admin.ConfirmAsync(ProbeHarness.MirrorSubscription, Ct)).ShouldBe(RebuildAdminOutcome.NotAwaitingConfirmation, "已完成的轮次不应再次被确认");
    }

    [Fact]
    public async Task Admin_Abandon_EndsTheRunAndStopsAutomaticRebuilds()
    {
        await using var harness = PausingHarness(database.ConnectionString);
        var runId = await PausedRunAsync(harness);
        var admin = harness.Services.GetRequiredService<IReadModelRebuildAdmin>();
        (await NeedsInitializationAsync()).ShouldBeTrue();

        (await admin.AbandonAsync(ProbeHarness.MirrorSubscription, Ct)).ShouldBe(RebuildAdminOutcome.Done);

        var run = await RunAsync(runId);
        run.Status.ShouldBe(RebuildRunStatus.Abandoned);
        run.FinishedAt.ShouldNotBeNull();
        (await NeedsInitializationAsync()).ShouldBeFalse("放弃重建即视为人工确认处理完毕，后续不再自动触发初始化排队");
        (await SummariesAsync()).Values.ShouldAllBe(r => !r.IsTerminal);
        (await admin.AbandonAsync(ProbeHarness.MirrorSubscription, Ct)).ShouldBe(RebuildAdminOutcome.NotAwaitingConfirmation);
    }

    [Fact]
    public async Task Admin_Trigger_QueuesOneRun_AndTheRunsAreListed()
    {
        Source.Set("一", version: 1, Id(1));
        var admin = _harness.Services.GetRequiredService<IReadModelRebuildAdmin>();

        var first = await admin.TriggerAsync(ProbeHarness.MirrorSubscription, Ct);
        var second = await admin.TriggerAsync(ProbeHarness.MirrorSubscription, Ct);
        var unknown = await admin.TriggerAsync("Echo.NoSuchSubscription", Ct);

        first.ShouldBe(RebuildAdminOutcome.Done);
        second.ShouldBe(RebuildAdminOutcome.AlreadyRunning);
        unknown.ShouldBe(RebuildAdminOutcome.UnknownSubscription);
        var listed = (await admin.ListAsync(Ct)).Single(r => string.Equals(r.Subscription, ProbeHarness.MirrorSubscription, StringComparison.Ordinal));
        listed.Module.ShouldBe(Module);
        listed.State.ShouldBe(RebuildRunState.Active);

        await _harness.Services.RunJobsAsync(Ct);
        (await admin.ListAsync(Ct)).Single(r => string.Equals(r.Subscription, ProbeHarness.MirrorSubscription, StringComparison.Ordinal)).State.ShouldBe(RebuildRunState.Completed);
    }

    [Fact]
    public async Task Shutdown_ReleasedThenResumesFromCheckpoint()
    {
        for (var n = 1; n <= 6; n++)
        {
            Source.Set($"单据{n}", version: 1, Id(n));
        }

        using var shutdown = new CancellationTokenSource();
        Source.BeforePage = async (page, ct) =>
        {
            if (page == 2)
            {
                await shutdown.CancelAsync();
                await UntilCancelledAsync(ct);
            }
        };

        var runId = await QueueAsync();
        var token = Guid.CreateVersion7();
        var claimed = (await Executor.ClaimAsync(Module, 1, token, Ct)).ShouldHaveSingleItem();
        (await Executor.RunAsync(Module, claimed, token, shutdown.Token)).ShouldBe(JobRunResult.Released);

        // 第一页数据已随检查点提交，第二页尚未开始处理
        (await SummariesAsync()).Keys.ShouldBe([Id(1), Id(2)]);
        (await JobStatusAsync((await RunAsync(runId)).JobId)).ShouldBe(JobStatus.Queued);

        Source.BeforePage = null;
        await _harness.Services.RunJobsAsync(Ct);

        (await SummariesAsync()).Count.ShouldBe(6);
        var run = await RunAsync(runId);
        (run.Status, run.ScannedCount).ShouldBe((RebuildRunStatus.Completed, 6L));
    }

    [Fact]
    public async Task TakenOverBeforePageCommit_PageRolledBack()
    {
        Source.Set("一", version: 1, Id(1));
        Source.Set("二", version: 1, Id(2));
        var runId = await QueueAsync();
        var jobId = (await RunAsync(runId)).JobId;
        Source.BeforePage = async (_, _) =>
        {
            await using var takeOver = database.DataSource.CreateCommand("""UPDATE "Echo"."BackgroundJob" SET "LeaseToken" = @other WHERE "Id" = @id""");
            takeOver.Parameters.AddWithValue("other", Guid.CreateVersion7());
            takeOver.Parameters.AddWithValue("id", jobId);
            await takeOver.ExecuteNonQueryAsync(CancellationToken.None);
        };

        var token = Guid.CreateVersion7();
        var claimed = (await Executor.ClaimAsync(Module, 1, token, Ct)).ShouldHaveSingleItem();
        (await Executor.RunAsync(Module, claimed, token, Ct)).ShouldBe(JobRunResult.TakenOver);

        (await SummariesAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Startup_QueuesOnceForNewSubscription_ClearedAfterCompletion()
    {
        Source.Set("一", version: 1, Id(1));

        await RunStartupTasksAsync();
        await RunStartupTasksAsync();

        var run = (await RunsAsync()).ShouldHaveSingleItem();
        (run.Status, run.Subscription, run.ReadModel).ShouldBe((RebuildRunStatus.Active, ProbeHarness.MirrorSubscription, nameof(TicketSummary)));
        (await JobStatusAsync(run.JobId)).ShouldBe(JobStatus.Queued);
        (await NeedsInitializationAsync()).ShouldBeTrue();

        await _harness.Services.RunJobsAsync(Ct);
        await RunStartupTasksAsync();

        (await RunsAsync()).ShouldHaveSingleItem().Status.ShouldBe(RebuildRunStatus.Completed);
        (await NeedsInitializationAsync()).ShouldBeFalse();
        (await SummariesAsync()).Keys.ShouldBe([Id(1)]);
    }

    [Fact]
    public async Task Startup_PreviousRunFailed_QueuesAnother()
    {
        await RunStartupTasksAsync();
        var first = (await RunsAsync()).ShouldHaveSingleItem();
        await using (var fail = database.DataSource.CreateCommand("""UPDATE "Echo"."BackgroundJob" SET "Status" = 3 WHERE "Id" = @id"""))
        {
            fail.Parameters.AddWithValue("id", first.JobId);
            await fail.ExecuteNonQueryAsync(Ct);
        }

        await RunStartupTasksAsync();

        var runs = await RunsAsync();
        runs.Count.ShouldBe(2);
        runs.Single(r => r.Id == first.Id).Status.ShouldBe(RebuildRunStatus.Failed);
        runs.Single(r => r.Id != first.Id).Status.ShouldBe(RebuildRunStatus.Active);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    // 按序号生成有序的单据 ID
    private static Guid Id(int n) => Guid.Parse($"00000000-0000-0000-0000-{n:D12}");

    private static TicketSummary Row(Guid id, string title, long version, ReferenceStatus status = ReferenceStatus.Active) =>
        new() { Id = id, Title = title, SourceVersion = version, Status = status };

    private static async Task UntilCancelledAsync(CancellationToken ct)
    {
        var cancelled = new TaskCompletionSource();
        await using var registration = ct.Register(() => cancelled.TrySetResult());
        await cancelled.Task;
        ct.ThrowIfCancellationRequested();
    }

    // 直接将重建轮次入队，绕过路由表中的初始化状态标记
    private async Task<Guid> QueueAsync(ProbeHarness? harness = null)
    {
        var queued = await (harness ?? _harness).Services.GetRequiredService<RebuildRunStore>()
            .QueueAsync(CallOrigin.Startup, Module, ProbeHarness.MirrorSubscription, nameof(TicketSummary), Ct);
        return queued ?? throw new InvalidOperationException("没有排上队。");
    }

    // 执行宿主启动阶段与重建相关的两个启动任务：注册消息路由并为未初始化的订阅排队重建
    private async Task RunStartupTasksAsync(ProbeHarness? harness = null)
    {
        await using var work = (harness ?? _harness).BeginAsSystem();
        var tasks = work.Services.GetServices<IStartupTask>()
            .Where(t => t.Name is "Framework.RegisterMessageRoutes" or "Framework.QueueReadModelRebuilds")
            .OrderBy(t => t.Order);
        foreach (var task in tasks)
        {
            (await task.ExecuteAsync(Ct)).IsSuccess.ShouldBeTrue();
        }
    }

    private async Task SeedAsync(Guid id, string title, long version) =>
        await WithWriterAsync(w => w.UpsertAsync(Row(id, title, version), Ct));

    private async Task<bool> WithWriterAsync(Func<IReadModelWriter<TicketSummary>, Task<bool>> write)
    {
        await using var work = _harness.BeginAsSystem();
        return await write(work.Services.GetRequiredService<IReadModelWriter<TicketSummary>>());
    }

    private async Task<Dictionary<Guid, TicketSummary>> SummariesAsync()
    {
        await using var work = _harness.BeginAsSystem();
        var rows = await work.Services.GetRequiredService<EchoQueryDbContext>().Tickets.AsNoTracking().ToListAsync(Ct);
        return rows.ToDictionary(r => r.Id);
    }

    private async Task<ReadModelRebuildRun> RunAsync(Guid runId) => (await RunsAsync()).Single(r => r.Id == runId);

    private async Task<List<ReadModelRebuildRun>> RunsAsync()
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Services.GetRequiredService<EchoDbContext>().Set<ReadModelRebuildRun>().AsNoTracking().OrderBy(r => r.StartedAt).ToListAsync(Ct);
    }

    private async Task<short> JobStatusAsync(Guid jobId)
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Services.GetRequiredService<EchoDbContext>().Set<BackgroundJobRecord>().AsNoTracking().Where(j => j.Id == jobId).Select(j => j.Status).SingleAsync(Ct);
    }

    private async Task<JsonElement> SummaryOfJobAsync(Guid jobId)
    {
        await using var work = _harness.BeginAsSystem();
        var summary = await work.Services.GetRequiredService<EchoDbContext>().Set<BackgroundJobRecord>().AsNoTracking()
            .Where(j => j.Id == jobId).Select(j => j.ResultSummary).SingleAsync(Ct);
        return JsonDocument.Parse(summary!).RootElement;
    }

    private async Task<bool> NeedsInitializationAsync()
    {
        await using var command = database.DataSource.CreateCommand(
            """SELECT "NeedsInitialization" FROM "Operations"."MessageRoute" WHERE "Subscription" = @subscription""");
        command.Parameters.AddWithValue("subscription", ProbeHarness.MirrorSubscription);
        return (bool)(await command.ExecuteScalarAsync(Ct))!;
    }
}
