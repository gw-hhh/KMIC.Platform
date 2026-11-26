using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Metrics;
using Kmic.BuildingBlocks.Testing.Scheduling;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Pipeline;

/// <summary>
/// 提交时连接断开：结果可能已生效也可能没有，各个入口都不能当成功或失败处理
/// </summary>
public sealed class CommitOutcomeUnknownTests : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Module = "BuildingBlocks";
    private const string InsertTicket = """INSERT INTO "Probe"."Ticket" """;
    private const string UpdateTicket = """UPDATE "Probe"."Ticket" """;
    private const string InsertLine = """INSERT INTO "Probe"."TicketLine" """;

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly DateTimeOffset _sevenAm = new(2026, 11, 10, 23, 0, 0, TimeSpan.Zero);

    private readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));
    private readonly CommitFaults _faults = new();
    private readonly ProbeHarness _harness;

    public CommitOutcomeUnknownTests(ClassDatabase<ProbeTemplate> database)
    {
        _harness = new ProbeHarness(database.ConnectionString, services: s => s
            .ConfigureDbContext<ProbeDbContext>(o => o.AddInterceptors(_faults))
            .Configure<JobOptions>(o => o.Lease = TimeSpan.FromMilliseconds(300)));
        _harness.Access.Register(_alice);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task IdempotentCommand_CommittedButNotAcknowledged_ReturnsTheCommittedResult()
    {
        var key = NewKey();
        var command = new OpenTicketCommand(TicketNumbers.Next(), "确认丢失");
        _faults.LoseAcknowledgement(InsertTicket);

        // 幂等记录和业务一起提交了：按记录判断出已经生效，直接按重放返回
        var first = await SendAsync(command, key);
        var retried = await SendAsync(command, key);

        _faults.Fired.ShouldBeTrue();
        first.Replayed.ShouldBeTrue();
        retried.Result.ShouldBeSuccess().ShouldBe(first.Result.ShouldBeSuccess());
        (await TicketCountAsync(command.Number)).ShouldBe(1);
        _harness.Hooks.Handled.ShouldBe(1);
    }

    [Fact]
    public async Task IdempotentCommand_CommitNeverSent_RetryableWithTheSameKey()
    {
        var key = NewKey();
        var command = new OpenTicketCommand(TicketNumbers.Next(), "没有提交");
        _faults.DropBeforeCommit(InsertTicket);

        var first = await SendAsync(command, key);
        (await TicketCountAsync(command.Number)).ShouldBe(0);
        var retried = await SendAsync(command, key);

        _faults.Fired.ShouldBeTrue();
        first.Result.ShouldBeFailure(CommonErrors.ResultUnknown).Metadata[ErrorMetadataKeys.Retryable].ShouldBe(true);
        retried.Result.ShouldBeSuccess();
        retried.Replayed.ShouldBeFalse();
        (await TicketCountAsync(command.Number)).ShouldBe(1);
    }

    [Fact]
    public async Task NonIdempotentCommand_NotAcknowledged_ResultUnknownAndNotRetryable()
    {
        using var metrics = new MetricRecorder(_harness.Services.GetRequiredService<IMeterFactory>());
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        _faults.LoseAcknowledgement(UpdateTicket);

        Result<VersionedResponse<TicketId>> result;
        await using (var work = _harness.Begin(_alice))
        {
            result = await work.Sender.SendAsync(new RenameTicketCommand(ticket.Id, 0, "改名后"), Ct);
        }

        // 不能让调用方直接重发：改名其实已经生效了，重发会按旧版本号报冲突或者重复执行
        _faults.Fired.ShouldBeTrue();
        result.ShouldBeFailure(CommonErrors.ResultUnknown).Metadata[ErrorMetadataKeys.Retryable].ShouldBe(false);
        (await TitleAsync(ticket.Id)).ShouldBe("改名后");
        metrics.Of("kmic.commit.outcome_unknown").ShouldHaveSingleItem().Tags["entry"].ShouldBe("Http");
    }

    [Fact]
    public async Task ScheduledRun_NotAcknowledged_RecordedAsUnknown()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        _harness.Hooks.StampTicket = ticket.Id;
        _faults.LoseAcknowledgement(InsertLine);

        var result = await _harness.Services.FireAsync(ProbeHarness.DailyStamp, _sevenAm, Ct);

        _faults.Fired.ShouldBeTrue();
        result.ShouldBeFailure(CommonErrors.ResultUnknown);
        (await LastRunStatusAsync(ProbeHarness.DailyStamp)).ShouldBe(ScheduledRunStatus.Unknown);
        (await LinesAsync(ticket.Id)).ShouldBe(["定时 23:00"]);
    }

    [Fact]
    public async Task JobBatch_NotAcknowledged_StopsWithoutFailing_ThenResumesFromTheCheckpoint()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        Guid jobId;
        await using (var work = _harness.Begin(_alice))
        {
            jobId = (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, 3, Export: false), Ct)).ShouldBeSuccess();
        }

        _faults.LoseAcknowledgement(InsertLine, occurrence: 2);
        var executor = _harness.Services.GetRequiredService<JobExecutor>();

        // 第 2 批其实提交了，但执行者不知道：不写失败、不发结束通知，作业停在运行中，等占用到期
        (await executor.RunBatchAsync(Module, 1, Ct)).ShouldBe(1);
        _faults.Fired.ShouldBeTrue();
        var stopped = await JobAsync(jobId);
        stopped.Status.ShouldBe(JobStatus.Running);
        stopped.ErrorCode.ShouldBeNull();
        (await LinesAsync(ticket.Id)).ShouldBe(["第 1 批", "第 2 批"]);

        // 占用到期后重新领取，从和第 2 批一起提交的检查点接着做，不重复第 2 批
        await Eventually.UntilAsync(
            async ct =>
            {
                await executor.RunBatchAsync(Module, 1, ct);
                return (await JobAsync(jobId)).Status == JobStatus.Succeeded;
            },
            "占用到期后作业续跑完成",
            ct: Ct);
        var finished = await JobAsync(jobId);
        finished.Attempts.ShouldBe(2);
        (await LinesAsync(ticket.Id)).ShouldBe(["第 1 批", "第 2 批", "第 3 批"]);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static string NewKey() => Guid.CreateVersion7().ToString();

    private Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);

    private async Task<(Result<CreatedResponse<TicketId>> Result, bool Replayed)> SendAsync(OpenTicketCommand command, string key)
    {
        await using var work = _harness.Begin(_alice);
        work.Request.IdempotencyKey = key;
        var result = await work.Sender.SendAsync(command, Ct);
        return (result, work.Request.Replayed);
    }

    private async Task<int> TicketCountAsync(string number)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<Ticket>().CountAsync(t => t.Number == number, Ct);
    }

    private async Task<string> TitleAsync(TicketId id)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<Ticket>().Where(t => t.Id == id).Select(t => t.Title).SingleAsync(Ct);
    }

    private async Task<string[]> LinesAsync(TicketId id)
    {
        await using var check = _harness.BeginAsSystem();
        var lines = await check.Db.Set<Ticket>().Where(t => t.Id == id).SelectMany(t => t.Lines).Select(l => l.Text).ToListAsync(Ct);
        return [.. lines.Order(StringComparer.Ordinal)];
    }

    private async Task<BackgroundJobRecord> JobAsync(Guid id)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<BackgroundJobRecord>().AsNoTracking().SingleAsync(j => j.Id == id, Ct);
    }

    private async Task<short> LastRunStatusAsync(string job)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Database
            .SqlQueryRaw<short>($$"""SELECT "Status" AS "Value" FROM {{SchedulingTables.Runs}} WHERE "JobName" = {0} ORDER BY "Id" DESC LIMIT 1""", job)
            .SingleAsync(Ct);
    }
}
