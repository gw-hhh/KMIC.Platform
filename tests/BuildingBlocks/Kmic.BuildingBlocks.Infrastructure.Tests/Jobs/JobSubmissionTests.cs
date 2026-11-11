using System.Text.Json;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Jobs;

/// <summary>
/// 每个测试用自己的用户，同时运行数量的上限互不影响
/// </summary>
public sealed class JobSubmissionTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly TestIdentity _user = TestIdentity.User("统计员").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Submit_QueuedRowWithSubmitter_CommittedWithCommand()
    {
        var ticket = await NewTicketAsync();

        var jobId = (await StartAsync(new StartRecountCommand(ticket.Id, Batches: 3))).ShouldBeSuccess();

        var job = await JobAsync(jobId);
        job.JobType.ShouldBe("BuildingBlocks.RecountTicket");
        job.SchemaVersion.ShouldBe(1);
        job.Status.ShouldBe(JobStatus.Queued);
        job.SubmittedBy.ShouldBe(_user.UserId.Value);
        job.SubmittedRoleId.ShouldBe(_user.RoleId.Value);
        job.SubmittedAt.ShouldBe(_harness.Time.GetUtcNow());
        var args = JsonDocument.Parse(job.Args).RootElement;
        args.GetProperty("ticketId").GetGuid().ShouldBe(ticket.Id.Value);
        args.GetProperty("batches").GetInt32().ShouldBe(3);
    }

    [Fact]
    public async Task CommandFails_JobRolledBack()
    {
        var ticket = await NewTicketAsync();

        (await StartAsync(new StartRecountCommand(ticket.Id, Fail: true))).ShouldBeFailure(TicketErrors.Rejected);

        await using var check = _harness.BeginAsSystem();
        (await check.Db.Set<BackgroundJobRecord>().AnyAsync(j => j.SubmittedBy == _user.UserId.Value, Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task UnfinishedAtLimit_TooManyJobs()
    {
        var ticket = await NewTicketAsync();
        (await StartAsync(new StartRecountCommand(ticket.Id))).ShouldBeSuccess();
        (await StartAsync(new StartRecountCommand(ticket.Id))).ShouldBeSuccess();

        (await StartAsync(new StartRecountCommand(ticket.Id))).ShouldBeFailure(CommonErrors.TooManyJobs);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private Task<Ticket> NewTicketAsync() => _harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));

    private async Task<Result<Guid>> StartAsync(StartRecountCommand command)
    {
        await using var work = _harness.Begin(_user);
        return await work.Sender.SendAsync(command, Ct);
    }

    private async Task<BackgroundJobRecord> JobAsync(Guid id)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<BackgroundJobRecord>().AsNoTracking().SingleAsync(j => j.Id == id, Ct);
    }
}
