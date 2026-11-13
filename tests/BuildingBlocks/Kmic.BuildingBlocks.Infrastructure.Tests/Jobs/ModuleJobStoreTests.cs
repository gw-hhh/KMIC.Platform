using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Jobs;

/// <summary>
/// Operations 汇总作业用的存储：按模块遍历、筛选、跨批翻页、看状态、请求取消
/// </summary>
public sealed class ModuleJobStoreTests : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Module = "BuildingBlocks";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));
    private readonly TestIdentity _bob = TestIdentity.User("乙").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));

    private readonly TestIdentity _carol = TestIdentity.User("丙").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));
    private readonly ProbeHarness _harness;

    public ModuleJobStoreTests(ClassDatabase<ProbeTemplate> database)
    {
        _harness = new ProbeHarness(database.ConnectionString);
        foreach (var user in new[] { _alice, _bob, _carol })
        {
            _harness.Access.Register(user);
        }
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IModuleJobStore Store => _harness.Services.GetRequiredService<IModuleJobStores>().Find(Module).ShouldNotBeNull();

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    [Fact]
    public void EveryModuleWithADatabaseHasAStore_InNameOrder()
    {
        var stores = _harness.Services.GetRequiredService<IModuleJobStores>();

        var names = stores.All.Select(s => s.Module).ToList();
        names.Contains(Module, StringComparer.Ordinal).ShouldBeTrue();
        names.ShouldBe(names.Order(StringComparer.Ordinal));
        stores.Find("NoSuchModule").ShouldBeNull();
    }

    [Fact]
    public async Task List_NewestFirst_FiltersAndPagesAcrossBatches()
    {
        // 每种作业每个用户同时最多两个，所以三个人分着提交
        var mine = new List<Guid> { await SubmitAsync(_alice), await SubmitAsync(_alice), await SubmitAsync(_carol) };

        var theirs = await SubmitAsync(_bob);
        HashSet<Guid> ids = [.. mine, theirs];

        // 同一个类里的测试共用一个库，别的测试提交的作业也在里面，所以只看这个测试自己提交的
        var all = await Store.ListAsync(JobFilter.None, null, 100, Ct);
        var onlyBob = await Store.ListAsync(new JobFilter(SubmittedBy: _bob.UserId.Value), null, 100, Ct);
        var queued = await Store.ListAsync(new JobFilter(State: BackgroundJobState.Queued), null, 100, Ct);
        var done = await Store.ListAsync(new JobFilter(State: BackgroundJobState.Succeeded), null, 100, Ct);
        var wrongType = await Store.ListAsync(new JobFilter(JobType: "BuildingBlocks.NoSuchJob"), null, 100, Ct);

        all.Select(j => j.SubmittedAt).ShouldBe(all.Select(j => j.SubmittedAt).OrderByDescending(t => t));
        all.Where(j => ids.Contains(j.Id)).Select(j => j.Id).ShouldBe([theirs, .. Enumerable.Reverse(mine)]);
        onlyBob.Select(j => j.Id).ShouldContain(theirs);
        onlyBob.ShouldAllBe(j => j.SubmittedBy == _bob.UserId.Value);
        queued.ShouldAllBe(j => j.State == BackgroundJobState.Queued);
        done.ShouldAllBe(j => j.State == BackgroundJobState.Succeeded);
        wrongType.ShouldBeEmpty();

        var seen = new List<Guid>();
        JobPosition? after = null;
        while (true)
        {
            var page = await Store.ListAsync(JobFilter.None, after, 3, Ct);
            seen.AddRange(page.Select(j => j.Id));
            if (page.Count < 3)
            {
                break;
            }

            after = new JobPosition(page[^1].SubmittedAt, page[^1].Id);
        }

        seen.ShouldBe(all.Select(j => j.Id));
    }

    [Fact]
    public async Task Find_ShowsProgressAndResult_ButNotTheArguments()
    {
        var id = await SubmitAsync(_alice, batches: 3);
        await _harness.Services.RunJobsAsync(Ct);

        var job = (await Store.FindAsync(id, Ct)).ShouldNotBeNull();

        job.Module.ShouldBe(Module);
        job.State.ShouldBe(BackgroundJobState.Succeeded);
        job.ProgressDone.ShouldBe(3);
        job.SubmittedBy.ShouldBe(_alice.UserId.Value);
        job.StartedAt.ShouldNotBeNull();
        job.FinishedAt.ShouldNotBeNull();
        job.ResultSummary.ShouldNotBeNull().ShouldContain("batches");
        job.Attempts.ShouldBe(1);
        job.ErrorCode.ShouldBeNull();
        (await Store.FindAsync(Guid.CreateVersion7(), Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task RequestCancel_QueuedJobIsCancelledAtOnce_AFinishedOneIsLeftAlone()
    {
        var finished = await SubmitAsync(_alice, batches: 1);
        await _harness.Services.RunJobsAsync(Ct);
        var queued = await SubmitAsync(_alice);

        var cancelled = await Store.RequestCancelAsync(queued, _bob.UserId, Ct);
        var tooLate = await Store.RequestCancelAsync(finished, _bob.UserId, Ct);

        cancelled.ShouldBeTrue();
        tooLate.ShouldBeFalse();
        var job = (await Store.FindAsync(queued, Ct)).ShouldNotBeNull();
        job.State.ShouldBe(BackgroundJobState.Cancelled);
        job.CancelRequestedBy.ShouldBe(_bob.UserId.Value);
        job.CancelRequestedAt.ShouldNotBeNull();
        (await Store.FindAsync(finished, Ct)).ShouldNotBeNull().State.ShouldBe(BackgroundJobState.Succeeded);
    }

    private async Task<Guid> SubmitAsync(TestIdentity user, int batches = 2)
    {
        var ticket = await _harness.AddAsync(user, Ticket.Create(TicketNumbers.Next(), user.UserId, _org["总部"]));
        await using var work = _harness.Begin(user);
        return (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id, batches, Export: false), Ct)).ShouldBeSuccess();
    }
}
