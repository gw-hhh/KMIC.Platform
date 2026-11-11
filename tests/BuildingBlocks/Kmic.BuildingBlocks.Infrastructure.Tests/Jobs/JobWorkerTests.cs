using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Jobs;

/// <summary>
/// 像宿主一样启动和停止后台服务，作业由执行循环自己领取
/// </summary>
public sealed class JobWorkerTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _user = TestIdentity.User("统计员").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Running_SubmittedJobPickedUpWithoutPolling()
    {
        // 兜底轮询设成很长，能及时执行只能是靠提交时的通知唤醒
        await using var harness = NewHarness();
        await harness.StartHostedServicesAsync(Ct);
        try
        {
            var (ticket, jobId) = await SubmitAsync(harness, batches: 2);

            await Eventually.UntilAsync(async ct => (await StatusAsync(harness, jobId, ct)) == JobStatus.Succeeded, "作业执行完", TimeSpan.FromSeconds(15), Ct);
            (await LinesAsync(harness, ticket)).ShouldBe(["第 1 批", "第 2 批"]);
        }
        finally
        {
            await harness.StopHostedServicesAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Stop_ResumableJobBackInQueue_WithCommittedBatchesKept()
    {
        await using var harness = NewHarness();
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        harness.Hooks.BeforeJobBatch = async (batch, ct) =>
        {
            if (batch == 1)
            {
                reached.TrySetResult();
                var stopped = new TaskCompletionSource();
                await using var registration = ct.Register(() => stopped.TrySetResult());
                await stopped.Task;
                ct.ThrowIfCancellationRequested();
            }
        };

        await harness.StartHostedServicesAsync(Ct);
        Guid jobId;
        Ticket ticket;
        try
        {
            (ticket, jobId) = await SubmitAsync(harness, batches: 3);
            await reached.Task.WaitAsync(TimeSpan.FromSeconds(15), Ct);
        }
        finally
        {
            await harness.StopHostedServicesAsync(CancellationToken.None);
        }

        (await StatusAsync(harness, jobId, Ct)).ShouldBe(JobStatus.Queued);
        (await LinesAsync(harness, ticket)).ShouldBe(["第 1 批"]);
    }

    private ProbeHarness NewHarness()
    {
        var harness = new ProbeHarness(database.ConnectionString, messaging: o => o.PollInterval = TimeSpan.FromMinutes(5));
        harness.Access.Register(_user);
        return harness;
    }

    private async Task<(Ticket Ticket, Guid JobId)> SubmitAsync(ProbeHarness harness, int batches)
    {
        var ticket = await harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        await using var work = harness.Begin(_user);
        var jobId = (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, batches), Ct)).ShouldBeSuccess();
        return (ticket, jobId);
    }

    private static async Task<short> StatusAsync(ProbeHarness harness, Guid id, CancellationToken ct)
    {
        await using var check = harness.BeginAsSystem();
        return await check.Db.Set<BackgroundJobRecord>().Where(j => j.Id == id).Select(j => j.Status).SingleAsync(ct);
    }

    private static async Task<string[]> LinesAsync(ProbeHarness harness, Ticket ticket)
    {
        await using var check = harness.BeginAsSystem();
        var lines = await check.Db.Set<Ticket>().Where(t => t.Id == ticket.Id).SelectMany(t => t.Lines).Select(l => l.Text).ToListAsync(Ct);
        return [.. lines.Order(StringComparer.Ordinal)];
    }
}
