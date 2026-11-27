using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Metrics;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Observability;

public sealed class BacklogSamplerTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const string Mirror = ProbeHarness.MirrorSubscription;
    private const string RecountJob = "BuildingBlocks.RecountTicket";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));

    private readonly ProbeHarness _harness = new(
        database.ConnectionString,
        services: s =>
        {
            s.AddKmicBacklogSampling();
            s.AddKmicBacklogGauges<QueueGauges>();
            s.AddLogging(logging => logging.AddFakeLogging());
        });

    private MetricRecorder _metrics = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private BacklogSampler Sampler => _harness.Services.GetRequiredService<BacklogSampler>();

    private QueueGauges Queue => _harness.Services.GetServices<IBacklogGauges>().OfType<QueueGauges>().Single();

    public async ValueTask InitializeAsync()
    {
        foreach (var table in new[] { "Probe.OutboxMessage", "Probe.BackgroundJob", "Probe.InboxMessage", "Echo.OutboxMessage", "Echo.InboxMessage", "Echo.BackgroundJob", "Operations.ScheduledJobRun", "Operations.MessageRoute" })
        {
            var parts = table.Split('.');
            await ExecuteAsync($"DELETE FROM \"{parts[0]}\".\"{parts[1]}\"");
        }

        _harness.Access.Register(_alice);
        _metrics = new MetricRecorder(_harness.Services.GetRequiredService<IMeterFactory>());
    }

    public async ValueTask DisposeAsync()
    {
        _metrics.Dispose();
        await _harness.DisposeAsync();
    }

    [Fact]
    public async Task NothingQueued_ReportsZeroForEveryModuleAndDeclaredSubscription()
    {
        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.outbox.pending", "module", "BuildingBlocks").ShouldBe(0);
        Of(gauges, "kmic.outbox.pending", "module", "Echo").ShouldBe(0);
        Of(gauges, "kmic.outbox.oldest_age", "module", "Echo").ShouldBe(0);
        Of(gauges, "kmic.inbox.pending", "subscription", Mirror).ShouldBe(0);
        Of(gauges, "kmic.inbox.dead", "subscription", Mirror).ShouldBe(0);
        Of(gauges, "kmic.inbox.blocked_keys", "subscription", Mirror).ShouldBe(0);
        gauges.Where(m => m.Instrument is "kmic.background_job.queued" or "kmic.scheduled_job.last_success_age").ShouldBeEmpty();
    }

    [Fact]
    public async Task UndeliveredMessages_ReportedPerModuleWithOldestAge()
    {
        await ShareTicketsAsync(2);
        await ExecuteAsync("UPDATE \"Probe\".\"OutboxMessage\" SET \"OccurredAt\" = clock_timestamp() - interval '10 minutes'");

        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.outbox.pending", "module", "BuildingBlocks").ShouldBe(2);
        Of(gauges, "kmic.outbox.oldest_age", "module", "BuildingBlocks").ShouldBeInRange(600, 660);
        Of(gauges, "kmic.outbox.pending", "module", "Echo").ShouldBe(0);
    }

    [Fact]
    public async Task DeliveredMessages_AppearInTheSubscribersInbox()
    {
        await ShareTicketsAsync(3);
        await DeliverAsync();
        await ExecuteAsync("UPDATE \"Echo\".\"InboxMessage\" SET \"ReceivedAt\" = clock_timestamp() - interval '5 minutes'");

        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.outbox.pending", "module", "BuildingBlocks").ShouldBe(0);
        Of(gauges, "kmic.inbox.pending", "subscription", Mirror).ShouldBe(3);
        Of(gauges, "kmic.inbox.oldest_age", "subscription", Mirror).ShouldBeInRange(300, 360);
        gauges.Where(m => string.Equals(m.Instrument, "kmic.inbox.pending", StringComparison.Ordinal)).ShouldAllBe(m => string.Equals(m.Tags["module"], "Echo", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DeadLetterOfAnOrderedSubscription_CountsDeadAndBlockedKeys()
    {
        await ShareTicketsAsync(3);
        await DeliverAsync();
        await ExecuteAsync("UPDATE \"Echo\".\"InboxMessage\" SET \"OrderingKey\" = 'k1'");
        await ExecuteAsync("UPDATE \"Echo\".\"InboxMessage\" SET \"Status\" = 3 WHERE \"Id\" IN (SELECT \"Id\" FROM \"Echo\".\"InboxMessage\" ORDER BY \"Seq\" LIMIT 2)");

        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.inbox.pending", "subscription", Mirror).ShouldBe(1);
        Of(gauges, "kmic.inbox.dead", "subscription", Mirror).ShouldBe(2);
        Of(gauges, "kmic.inbox.blocked_keys", "subscription", Mirror).ShouldBe(1);
    }

    [Fact]
    public async Task RetiringSubscription_BacklogReportedSeparately()
    {
        await ShareTicketsAsync(2);
        await DeliverAsync();
        await ExecuteAsync("UPDATE \"Operations\".\"MessageRoute\" SET \"Status\" = 1 WHERE \"Subscription\" = 'Echo.MirrorTicket'");

        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.inbox.retiring_pending", "subscription", Mirror).ShouldBe(2);
        gauges.Where(m => m.Instrument is "kmic.inbox.pending" or "kmic.inbox.oldest_age").ShouldBeEmpty();
        Of(gauges, "kmic.inbox.dead", "subscription", Mirror).ShouldBe(0);
    }

    [Fact]
    public async Task QueuedJobs_CountedByTypeAndRunningOnesLeftOut()
    {
        await StartJobAsync();
        await StartJobAsync();
        await ExecuteAsync("UPDATE \"Probe\".\"BackgroundJob\" SET \"SubmittedAt\" = clock_timestamp() - interval '7 minutes'");

        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.background_job.queued", "type", RecountJob).ShouldBe(2);
        Of(gauges, "kmic.background_job.oldest_queued", "type", RecountJob).ShouldBeInRange(420, 480);
        gauges.Where(m => string.Equals(m.Instrument, "kmic.background_job.queued", StringComparison.Ordinal)).ShouldAllBe(m => string.Equals(m.Tags["module"], "BuildingBlocks", StringComparison.Ordinal));

        await ExecuteAsync("UPDATE \"Probe\".\"BackgroundJob\" SET \"Status\" = 1 WHERE \"Id\" = (SELECT \"Id\" FROM \"Probe\".\"BackgroundJob\" LIMIT 1)");
        _metrics.Clear();
        await Sampler.SampleAsync(Ct);
        Of(Collect(), "kmic.background_job.queued", "type", RecountJob).ShouldBe(1);
    }

    [Fact]
    public async Task ScheduledTask_AgeIsMeasuredFromTheLastSuccess()
    {
        await InsertRunAsync("Probe.Task", status: 1, finished: "2 hours");
        await InsertRunAsync("Probe.Task", status: 1, finished: "1 hour");
        await InsertRunAsync("Probe.Task", status: 2, finished: "10 minutes");
        await InsertRunAsync("Probe.Other", status: 2, finished: "10 minutes");

        await Sampler.SampleAsync(Ct);

        var gauges = Collect();
        Of(gauges, "kmic.scheduled_job.last_success_age", "task", "Probe.Task").ShouldBeInRange(3600, 3660);
        gauges.Where(m => string.Equals(m.Instrument, "kmic.scheduled_job.last_success_age", StringComparison.Ordinal)).ShouldHaveSingleItem();
    }

    [Fact]
    public void NoSampleYet_GaugesReportNothing() => Collect().ShouldBeEmpty();

    [Fact]
    public async Task StaleSample_StopsReportingUntilRefreshed()
    {
        await Sampler.SampleAsync(Ct);
        Collect().ShouldNotBeEmpty();

        // 默认采样间隔为 30 秒，超过三个采样周期未更新则视为样本过期
        _harness.Time.Advance(TimeSpan.FromSeconds(91));
        Sampler.Latest.ShouldBeNull();
        Collect().ShouldBeEmpty();

        await Sampler.SampleAsync(Ct);
        Collect().ShouldNotBeEmpty();
    }

    [Fact]
    public async Task BackgroundLoop_SamplesOnEveryTick_AndSurvivesFailures()
    {
        _harness.Services.GetRequiredService<StartupState>().Complete();
        var interval = _harness.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseDiagnosticsOptions>>().Value.BacklogSampleInterval;
        await Sampler.StartAsync(Ct);
        try
        {
            await Eventually.UntilAsync(_ => Task.FromResult(Sampler.Latest is not null), "后台服务启动后应立即执行首次采样", ct: Ct);

            // 临时重命名 Echo 的 Inbox 表以触发下一轮采样失败，验证系统记录警告日志且采样循环不中断
            await ExecuteAsync("ALTER TABLE \"Echo\".\"InboxMessage\" RENAME TO \"InboxMessageMoved\"");
            try
            {
                _harness.Time.Advance(interval);
                await Eventually.UntilAsync(
                    _ => Task.FromResult(_harness.Services.GetFakeLogCollector().GetSnapshot().Any(r => r.Id.Id == 2802)),
                    "采样失败后应记录对应的警告日志",
                    ct: Ct);
            }
            finally
            {
                await ExecuteAsync("ALTER TABLE \"Echo\".\"InboxMessageMoved\" RENAME TO \"InboxMessage\"");
            }

            await ShareTicketsAsync(1);
            _harness.Time.Advance(interval);
            await Eventually.UntilAsync(
                _ => Task.FromResult(Sampler.Latest?.Outbox.Single(o => string.Equals(o.Module, "BuildingBlocks", StringComparison.Ordinal)).Pending == 1),
                "恢复表名后的下一轮采样应反映最新积压数据",
                ct: Ct);
        }
        finally
        {
            await Sampler.StopAsync(Ct);
        }
    }

    [Fact]
    public async Task ModuleGauges_ReportTheirLatestReadings()
    {
        Queue.Next = 4;

        await Sampler.SampleModulesAsync(Ct);

        Of(Collect(), QueueGauges.Name, "channel", "Email").ShouldBe(4);
    }

    [Fact]
    public async Task ModuleGauges_FailedRead_KeepsThePreviousReadingUntilItExpires()
    {
        Queue.Next = 2;
        await Sampler.SampleModulesAsync(Ct);

        Queue.Next = null;
        _harness.Time.Advance(TimeSpan.FromSeconds(60));
        await Sampler.SampleModulesAsync(Ct);

        _harness.Services.GetFakeLogCollector().GetSnapshot().ShouldContain(r => r.Id.Id == 2804);
        Of(Collect(), QueueGauges.Name, "channel", "Email").ShouldBe(2);

        _harness.Time.Advance(TimeSpan.FromSeconds(31));
        Collect().Where(m => string.Equals(m.Instrument, QueueGauges.Name, StringComparison.Ordinal)).ShouldBeEmpty();
    }

    [Fact]
    public void GaugeRegistration_OnlyOnce()
    {
        _ = Sampler;
        var metrics = _harness.Services.GetRequiredService<FrameworkMetrics>();

        Should.Throw<InvalidOperationException>(() => metrics.ObserveBacklog(() => null));
    }

    private static double Of(IReadOnlyList<RecordedMeasurement> gauges, string instrument, string tag, string value) =>
        gauges.Where(m => string.Equals(m.Instrument, instrument, StringComparison.Ordinal) && m.Tags.TryGetValue(tag, out var found) && string.Equals(found, value, StringComparison.Ordinal)).ShouldHaveSingleItem().Value;

    private IReadOnlyList<RecordedMeasurement> Collect()
    {
        // 触发采样器初始化以确保相关指标量表完成注册
        _ = Sampler;
        _metrics.Clear();
        _metrics.Collect();
        return [.. _metrics.All];
    }

    private async Task ShareTicketsAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
            await using var work = _harness.Begin(_alice);
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["已分享"]), Ct)).ShouldBeSuccess();
        }
    }

    private async Task StartJobAsync()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await using var work = _harness.Begin(_alice);
        (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, 1), Ct)).ShouldBeSuccess();
    }

    // 仅将消息投递至 Inbox 而不消费处理，以保留 Inbox 积压状态
    private async Task DeliverAsync()
    {
        var subscriptions = _harness.Services.GetRequiredService<SubscriptionCatalog>();
        await new MessageRoutes(database.DataSource).RegisterAsync(MessageRoutes.Declared(subscriptions), MessageRoutes.Retirements(subscriptions), "test", Ct);
        var dispatcher = _harness.Services.GetRequiredService<OutboxDispatcher>();
        while (await dispatcher.DeliverBatchAsync("BuildingBlocks", Ct) > 0)
        {
        }
    }

    private Task InsertRunAsync(string job, short status, string finished) =>
        ExecuteAsync(
            $"""
            INSERT INTO "Operations"."ScheduledJobRun" ("Id", "JobName", "ScheduledFireTime", "ActualFireTime", "Trigger", "StartedAt", "FinishedAt", "Status", "InstanceId")
            VALUES (gen_random_uuid(), '{job}', clock_timestamp(), clock_timestamp(), 0, clock_timestamp() - interval '{finished}', clock_timestamp() - interval '{finished}', {status}, 'test')
            """);

    private async Task ExecuteAsync(string sql)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(Ct);
    }

    // 模块指标量表测试替身：当 Next 为 null 时模拟读取异常
    private sealed class QueueGauges : IBacklogGauges
    {
        public const string Name = "kmic.test.queue";

        public double? Next { get; set; }

        public string MeterName => "Kmic.Test";

        public IReadOnlyList<BacklogGauge> Gauges { get; } = [new(Name, "{item}", "测试队列的积压")];

        public Task<IReadOnlyList<BacklogReading>> ReadAsync(CancellationToken ct) =>
            Next is { } value
                ? Task.FromResult<IReadOnlyList<BacklogReading>>([new BacklogReading(Name, value, [new("channel", "Email")])])
                : throw new InvalidOperationException("读取失败");
    }
}
