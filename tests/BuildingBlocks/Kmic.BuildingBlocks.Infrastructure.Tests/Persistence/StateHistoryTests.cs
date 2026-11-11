using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.History;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 每个测试用自己的评审，按聚合 ID 读历史
/// </summary>
public sealed class StateHistoryTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task EveryTransition_OneRow_InOrder()
    {
        var review = Review.Create("评审一", reason: "新建");
        await SaveAsNewAsync(_alice, review);

        await UpdateAsync(_alice, review.Id, r => r.Submit("可以看了"));
        await UpdateAsync(_alice, review.Id, r => r.Approve());

        var rows = await HistoryAsync(review.Id);
        rows.Select(r => (r.FromCode, r.ToCode, r.ActionCode)).ShouldBe(
            [(null, "Draft", "Create"), ("Draft", "Submitted", "Submit"), ("Submitted", "Approved", "Approve")]);
        rows.Select(r => r.Reason).ShouldBe(["新建", "可以看了", null]);
        rows.ShouldAllBe(r => r.StateName == "Status" && r.AggregateId == review.Id.Value);
        rows.ShouldAllBe(r => r.ActorUserId == _alice.UserId.Value && r.OccurredAt == _harness.Time.GetUtcNow() && r.TraceId.Length == 32);
        rows.Select(r => r.Revision).ShouldBe([1, 1, 1]);
    }

    [Fact]
    public async Task SystemIdentity_ActorEmpty_OnBehalfOfRecorded()
    {
        var review = Review.Create("评审二");
        await SaveAsNewAsync(_alice, review);

        await using (var work = _harness.Begin(CallOrigin.Inbox, CurrentUser.System(onBehalfOf: _alice.UserId)))
        {
            var loaded = await work.Db.Set<Review>().SingleAsync(r => r.Id == review.Id, Ct);
            loaded.Submit();
            await work.SaveAsync();
        }

        var row = (await HistoryAsync(review.Id)).Last();
        row.ActorUserId.ShouldBeNull();
        row.OnBehalfOfUserId.ShouldBe(_alice.UserId.Value);
    }

    [Fact]
    public async Task RolledBack_NoHistory()
    {
        var review = Review.Create("评审三");
        await using (var work = _harness.Begin(_alice))
        {
            await using var transaction = await work.Db.Database.BeginTransactionAsync(Ct);
            work.Db.Add(review);
            await work.SaveAsync();
            await transaction.RollbackAsync(Ct);
        }

        (await HistoryAsync(review.Id)).ShouldBeEmpty();
    }

    [Fact]
    public async Task NoTransition_NoRow()
    {
        var review = Review.Create("评审四");
        await SaveAsNewAsync(_alice, review);

        await UpdateAsync(_alice, review.Id, r => r.Rename("改个名"));

        (await HistoryAsync(review.Id)).ShouldHaveSingleItem().ActionCode.ShouldBe("Create");
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task SaveAsNewAsync(TestIdentity identity, Review review)
    {
        await using var work = _harness.Begin(identity);
        work.Db.Add(review);
        await work.SaveAsync();
    }

    private async Task UpdateAsync(TestIdentity identity, ReviewId id, Action<Review> change)
    {
        await using var work = _harness.Begin(identity);
        var loaded = await work.Db.Set<Review>().SingleAsync(r => r.Id == id, Ct);
        change(loaded);
        await work.SaveAsync();
    }

    private async Task<List<StateHistoryRecord>> HistoryAsync(ReviewId id)
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Db.Set<StateHistoryRecord>(StateHistory.TableName(typeof(Review)))
            .AsNoTracking()
            .Where(r => r.AggregateId == id.Value)
            .OrderBy(r => r.Id)
            .ToListAsync(Ct);
    }
}
