using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Jobs;

/// <summary>
/// 作业每一批给单据加一行“第 N 批”。占用设得很短，续期每 100 毫秒一次，取消、超时、接管都能很快被发现
/// </summary>
public sealed class JobExecutionTests : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Module = "BuildingBlocks";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly ClassDatabase<ProbeTemplate> _database;
    private readonly TestIdentity _user = TestIdentity.User("统计员").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));
    private readonly ProbeHarness _harness;

    public JobExecutionTests(ClassDatabase<ProbeTemplate> database)
    {
        _database = database;
        _harness = NewHarness();
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private JobExecutor Executor => _harness.Services.GetRequiredService<JobExecutor>();

    [Fact]
    public async Task Run_EveryBatchCommittedWithCheckpoint_Succeeded()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 3);

        await _harness.Services.RunJobsAsync(Ct);

        var job = await JobAsync(jobId);
        job.Status.ShouldBe(JobStatus.Succeeded);
        job.ProgressDone.ShouldBe(3);
        JsonDocument.Parse(job.Checkpoint!).RootElement.GetProperty("nextBatch").GetInt32().ShouldBe(3);
        JsonDocument.Parse(job.ResultSummary!).RootElement.GetProperty("batches").GetInt32().ShouldBe(3);
        job.ExpiresAt.ShouldNotBeNull();
        job.Attempts.ShouldBe(1);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批", "第 2 批", "第 3 批"]);
    }

    [Fact]
    public async Task SubmitterDisabled_FailsWithoutRunning()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 2);
        _harness.Access.Remove(_user);

        await _harness.Services.RunJobsAsync(Ct);

        (await JobAsync(jobId)).ErrorCode.ShouldBe(CommonErrors.JobSubmitterUnavailable.Code);
        (await LinesAsync(ticket)).ShouldBeEmpty();
    }

    [Fact]
    public async Task SubmitterLostJobPermission_FailsWithoutRunning()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 2);
        _harness.Access.Register(_user with { Permissions = new HashSet<string>(StringComparer.Ordinal) });

        await _harness.Services.RunJobsAsync(Ct);

        (await JobAsync(jobId)).ErrorCode.ShouldBe(CommonErrors.JobSubmitterUnavailable.Code);
        (await LinesAsync(ticket)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FailureOrException_Failed_EarlierBatchesKept(bool throws)
    {
        if (throws)
        {
            _harness.Hooks.JobThrowAtBatch = 1;
        }
        else
        {
            _harness.Hooks.JobFailAtBatch = 1;
        }

        var (ticket, jobId) = await SubmitAsync(batches: 3);

        await _harness.Services.RunJobsAsync(Ct);

        var job = await JobAsync(jobId);
        job.Status.ShouldBe(JobStatus.Failed);
        job.ErrorCode.ShouldBe(throws ? CommonErrors.JobFailed.Code : TicketErrors.Rejected.Code);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批"]);
    }

    [Fact]
    public async Task CancelWhileQueued_NeverRuns()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 2);

        (await Executor.RequestCancelAsync(Module, jobId, _user.UserId, Ct)).ShouldBeTrue();
        await _harness.Services.RunJobsAsync(Ct);

        var job = await JobAsync(jobId);
        job.Status.ShouldBe(JobStatus.Cancelled);
        job.CancelRequestedBy.ShouldBe(_user.UserId.Value);
        (await LinesAsync(ticket)).ShouldBeEmpty();
    }

    [Fact]
    public async Task CancelWhileRunning_StopsAtBatchBoundary()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 3);
        _harness.Hooks.BeforeJobBatch = async (batch, ct) =>
        {
            if (batch == 1)
            {
                await Executor.RequestCancelAsync(Module, jobId, _user.UserId, CancellationToken.None);
                await UntilCancelledAsync(ct);
            }
        };

        await _harness.Services.RunJobsAsync(Ct);

        (await JobAsync(jobId)).Status.ShouldBe(JobStatus.Cancelled);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批"]);
    }

    [Fact]
    public async Task RunningTooLong_FailedAsTimedOut()
    {
        await using var harness = NewHarness(o => o.MaxRunTime = TimeSpan.FromMilliseconds(200));
        var (ticket, jobId) = await SubmitAsync(batches: 2, harness);
        harness.Hooks.BeforeJobBatch = async (batch, ct) =>
        {
            if (batch == 1)
            {
                await UntilCancelledAsync(ct);
            }
        };

        await harness.Services.RunJobsAsync(Ct);

        (await JobAsync(jobId)).ErrorCode.ShouldBe(CommonErrors.JobTimedOut.Code);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批"]);
    }

    [Fact]
    public async Task SubmitterDisabledWhileRunningLong_FailsAtBatchBoundary()
    {
        await using var harness = NewHarness(o => o.SubmitterCheckAfter = TimeSpan.Zero);
        var (ticket, jobId) = await SubmitAsync(batches: 2, harness);
        harness.Hooks.BeforeJobBatch = async (batch, ct) =>
        {
            if (batch == 1)
            {
                harness.Access.Remove(_user);
                await UntilCancelledAsync(ct);
            }
        };

        await harness.Services.RunJobsAsync(Ct);

        (await JobAsync(jobId)).ErrorCode.ShouldBe(CommonErrors.JobSubmitterUnavailable.Code);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批"]);
    }

    [Fact]
    public async Task TakenOverMidway_BatchRejected_NothingWritten()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 3);
        var other = Guid.CreateVersion7();
        _harness.Hooks.BeforeJobBatch = async (batch, ct) =>
        {
            if (batch == 1)
            {
                await ExecuteAsync("""UPDATE "Probe"."BackgroundJob" SET "LeaseToken" = @other WHERE "Id" = @id""", jobId, other);
            }
        };

        var token = Guid.CreateVersion7();
        var claimed = (await Executor.ClaimAsync(Module, 1, token, Ct)).ShouldHaveSingleItem();
        (await Executor.RunAsync(Module, claimed, token, Ct)).ShouldBe(JobRunResult.TakenOver);

        var job = await JobAsync(jobId);
        job.Status.ShouldBe(JobStatus.Running);
        job.LeaseToken.ShouldBe(other);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批"]);

        // 收尾：不让这条运行中的记录过期后被同一个库里后面的测试领走
        await ExecuteAsync("""UPDATE "Probe"."BackgroundJob" SET "Status" = 3, "LeaseToken" = NULL WHERE "Id" = @id""", jobId);
    }

    [Fact]
    public async Task Shutdown_ResumableReleased_ThenResumesFromCheckpoint()
    {
        var (ticket, jobId) = await SubmitAsync(batches: 3);
        using var shutdown = new CancellationTokenSource();
        _harness.Hooks.BeforeJobBatch = async (batch, ct) =>
        {
            if (batch == 2)
            {
                await shutdown.CancelAsync();
                await UntilCancelledAsync(ct);
            }
        };

        var token = Guid.CreateVersion7();
        var claimed = (await Executor.ClaimAsync(Module, 1, token, Ct)).ShouldHaveSingleItem();
        (await Executor.RunAsync(Module, claimed, token, shutdown.Token)).ShouldBe(JobRunResult.Released);
        (await JobAsync(jobId)).Status.ShouldBe(JobStatus.Queued);

        _harness.Hooks.BeforeJobBatch = null;
        await _harness.Services.RunJobsAsync(Ct);

        (await JobAsync(jobId)).Status.ShouldBe(JobStatus.Succeeded);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批", "第 2 批", "第 3 批"]);
    }

    [Fact]
    public async Task ExecutorLost_ResumableResumes_OthersMarkedInterrupted()
    {
        var (ticket, resumable) = await SubmitAsync(batches: 3);
        var (_, export) = await SubmitAsync(batches: 1, export: true);

        // 两个作业都被领走后执行者失联：一个做完了两批，一个还没开始
        var lost = Guid.CreateVersion7();
        (await Executor.ClaimAsync(Module, 2, lost, Ct)).Count.ShouldBe(2);
        await using (var work = _harness.Begin(_user))
        {
            (await work.Sender.SendAsync(new AnnotateTicketCommand(ticket.Id, "第 1 批"), Ct)).ShouldBeSuccess();
            (await work.Sender.SendAsync(new AnnotateTicketCommand(ticket.Id, "第 2 批"), Ct)).ShouldBeSuccess();
        }

        await ExecuteAsync("""UPDATE "Probe"."BackgroundJob" SET "Checkpoint" = '{"nextBatch":2}' WHERE "Id" = @id""", resumable);
        await ExecuteAsync("""UPDATE "Probe"."BackgroundJob" SET "LeaseUntil" = clock_timestamp() - interval '1 second' WHERE "Id" IN (@id, @other)""", resumable, export);

        await _harness.Services.RunJobsAsync(Ct);

        (await JobAsync(resumable)).Status.ShouldBe(JobStatus.Succeeded);
        (await LinesAsync(ticket)).ShouldBe(["第 1 批", "第 2 批", "第 3 批"]);
        (await JobAsync(export)).ErrorCode.ShouldBe(CommonErrors.JobInterrupted.Code);
    }

    [Fact]
    public async Task Shutdown_NotResumable_MarkedInterrupted()
    {
        var (_, jobId) = await SubmitAsync(batches: 1, export: true);
        using var shutdown = new CancellationTokenSource();
        _harness.Hooks.BeforeJobBatch = async (_, ct) =>
        {
            await shutdown.CancelAsync();
            await UntilCancelledAsync(ct);
        };

        var token = Guid.CreateVersion7();
        var claimed = (await Executor.ClaimAsync(Module, 1, token, Ct)).ShouldHaveSingleItem();
        (await Executor.RunAsync(Module, claimed, token, shutdown.Token)).ShouldBe(JobRunResult.Failed);

        (await JobAsync(jobId)).ErrorCode.ShouldBe(CommonErrors.JobInterrupted.Code);
    }


    [Fact]
    public async Task AuditedJob_OneAuditAtTheEnd_WithStatusAndSummary()
    {
        var (_, jobId) = await SubmitAsync(batches: 1, export: true);

        await _harness.Services.RunJobsAsync(Ct);

        await using var check = _harness.BeginAsSystem();
        var rows = await check.Db.Set<OutboxMessage>().AsNoTracking().Where(m => m.MessageType == "Framework.AuditRecorded").ToListAsync(Ct);
        var audit = rows
            .Select(r => JsonSerializer.Deserialize<AuditRecorded>(r.Payload, KmicJsonOptions.Stored)!)
            .Single(a => string.Equals(a.Details.GetValueOrDefault("jobId"), jobId.ToString("D"), StringComparison.Ordinal));
        audit.Action.ShouldBe("BuildingBlocks.ExportTicket");
        audit.Details["status"].ShouldBe("Succeeded");
        audit.Details.ContainsKey("summary").ShouldBeTrue();
        audit.ActorUserId.ShouldBe(_user.UserId.Value);
        audit.Changes.ShouldBeEmpty();
    }
    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static async Task UntilCancelledAsync(CancellationToken ct)
    {
        var cancelled = new TaskCompletionSource();
        await using var registration = ct.Register(() => cancelled.TrySetResult());
        await cancelled.Task;
        ct.ThrowIfCancellationRequested();
    }

    private ProbeHarness NewHarness(Action<JobOptions>? configure = null)
    {
        var harness = new ProbeHarness(_database.ConnectionString, services: s => s.Configure<JobOptions>(o =>
        {
            o.Lease = TimeSpan.FromMilliseconds(300);
            configure?.Invoke(o);
        }));
        harness.Access.Register(_user);
        return harness;
    }

    private async Task<(Ticket Ticket, Guid JobId)> SubmitAsync(int batches, ProbeHarness? harness = null, bool export = false)
    {
        harness ??= _harness;
        var ticket = await harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        await using var work = harness.Begin(_user);
        var jobId = (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, batches, Export: export), Ct)).ShouldBeSuccess();
        return (ticket, jobId);
    }

    private async Task<BackgroundJobRecord> JobAsync(Guid id)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<BackgroundJobRecord>().AsNoTracking().SingleAsync(j => j.Id == id, Ct);
    }

    private async Task<string[]> LinesAsync(Ticket ticket)
    {
        await using var check = _harness.BeginAsSystem();
        var lines = await check.Db.Set<Ticket>().Where(t => t.Id == ticket.Id).SelectMany(t => t.Lines).Select(l => l.Text).ToListAsync(Ct);
        return [.. lines.Order(StringComparer.Ordinal)];
    }

    private async Task ExecuteAsync(string sql, Guid id, Guid? other = null)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        if (other is { } value)
        {
            command.Parameters.AddWithValue("other", value);
        }

        await command.ExecuteNonQueryAsync(Ct);
    }
}
