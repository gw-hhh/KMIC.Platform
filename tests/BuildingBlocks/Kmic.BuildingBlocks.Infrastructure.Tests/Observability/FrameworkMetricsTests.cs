using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Jobs;
using Kmic.BuildingBlocks.Testing.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Observability;

/// <summary>
/// 每个测试用自己的服务，指标按各自的指标工厂记录，不受并行的其他测试影响
/// </summary>
public sealed class FrameworkMetricsTests : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    // 框架指标允许的维度：全是取值有限的名字，不能有用户、资源、追踪 ID
    private static readonly HashSet<string> _allowedTags = new(StringComparer.Ordinal)
    {
        "module", "message", "result", "reason", "type", "severity", "entry", "subscription", "task", "readmodel",
    };

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));

    private readonly ProbeHarness _harness;
    private readonly MetricRecorder _metrics;

    public FrameworkMetricsTests(ClassDatabase<ProbeTemplate> database)
    {
        _harness = new ProbeHarness(database.ConnectionString);
        _metrics = new MetricRecorder(_harness.Services.GetRequiredService<IMeterFactory>());
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CommandAndQuery_RecordCountAndDuration_WithModuleMessageAndResult()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new GetTicketTitleQuery(ticket.Id), Ct)).ShouldBeSuccess();
            (await work.Sender.SendAsync(new GetTicketTitleQuery(TicketId.New()), Ct)).ShouldBeFailure(TicketErrors.NotFound);
        }

        var requests = _metrics.Of("kmic.mediator.requests");
        requests.Select(r => r.Tags["result"]).ShouldBe(["success", TicketErrors.NotFound.Code]);
        requests.ShouldAllBe(r => r.Tags["module"] == "BuildingBlocks" && r.Tags["message"] == "BuildingBlocks.GetTicketTitleQuery");
        _metrics.Of("kmic.mediator.request.duration").Count.ShouldBe(2);
    }

    [Fact]
    public async Task AuthorizationDenied_CountedWithReason()
    {
        await using var work = _harness.Begin(_alice);

        var denied = await work.Sender.SendAsync(new StampTicketsCommand(DateTimeOffset.UnixEpoch), Ct);
        denied.ShouldBeFailure(Kmic.BuildingBlocks.Domain.Errors.CommonErrors.MissingPermission);
        _metrics.Of("kmic.authz.denied").ShouldHaveSingleItem().Tags["reason"].ShouldBe("SystemOnly");
    }

    [Fact]
    public async Task BackgroundJob_RecordsDurationAndFailures()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        _harness.Access.Register(_alice);
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, 1), Ct)).ShouldBeSuccess();
        }

        _harness.Hooks.JobFailAtBatch = 0;
        await _harness.Services.RunJobsAsync(Ct);

        var finished = _metrics.Of("kmic.background_job.duration").ShouldHaveSingleItem();
        finished.Tags["result"].ShouldBe("Failed");
        finished.Tags["type"].ShouldBe("BuildingBlocks.RecountTicket");
        _metrics.Of("kmic.background_job.failures").ShouldHaveSingleItem();
    }

    [Fact]
    public async Task EveryDimensionKey_IsOnTheAllowList()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await using (var work = _harness.Begin(_alice))
        {
            await work.Sender.SendAsync(new GetTicketTitleQuery(ticket.Id), Ct);
            await work.Sender.SendAsync(new StampTicketsCommand(DateTimeOffset.UnixEpoch), Ct);
        }

        var keys = _metrics.All.SelectMany(m => m.Tags.Keys).Distinct(StringComparer.Ordinal).ToList();

        keys.ShouldNotBeEmpty();
        keys.ShouldAllBe(key => _allowedTags.Contains(key));
    }

    public async ValueTask DisposeAsync()
    {
        _metrics.Dispose();
        await _harness.DisposeAsync();
    }
}
