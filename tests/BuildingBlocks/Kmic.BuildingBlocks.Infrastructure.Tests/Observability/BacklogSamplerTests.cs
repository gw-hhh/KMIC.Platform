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

/// <summary>
/// 积压采样与量表。每个测试开始前清空消息、作业、执行记录和路由，数据库时间用 SQL 直接改，年龄才可以断言
/// </summary>
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
            s.AddLogging(logging => logging.AddFakeLogging());
        });

    private MetricRecorder _metrics = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private BacklogSampler Sampler => _harness.Services.GetRequiredService<BacklogSampler>();

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

        // 默认采样间隔 30 秒，超过三个间隔没有更新就当作失效
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
            await Eventually.UntilAsync(_ => Task.FromResult(Sampler.Latest is not null), "启动后立即采样一次", ct: Ct);

            // 把 Echo 的 Inbox 改名：下一轮采样会失败，要记一条警告，循环继续
            await ExecuteAsync("ALTER TABLE \"Echo\".\"InboxMessage\" RENAME TO \"InboxMessageMoved\"");
            try
            {
                _harness.Time.Advance(interval);
                await Eventually.UntilAsync(
                    _ => Task.FromResult(_harness.Services.GetFakeLogCollector().GetSnapshot().Any(r => r.Id.Id == 2802)),
                    "采样失败要记警告",
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
                "恢复后的下一轮采样要反映新数据",
                ct: Ct);
        }
        finally
        {
            await Sampler.StopAsync(Ct);
        }
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
        // 登记量表的是采样器，先取一次让它建起来
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

    // 只投递到 Inbox，不处理：处理了就不再积压
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
}
