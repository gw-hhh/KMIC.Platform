using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Querying;

/// <summary>
/// 分页在真实 PostgreSQL 上执行：每个测试建 25 张标题带唯一前缀的单据，金额只有五种，大量同值
/// </summary>
public sealed class PagingTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const int Count = 25;

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private static readonly SortMap<TicketListItem> _sort = SortMap<TicketListItem>
        .Field("number", x => x.Number)
        .Field("title", x => x.Title)
        .Field("amount", x => x.Amount)
        .Default("title");

    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly string _prefix = $"P{Guid.CreateVersion7():N}-";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        for (var i = 0; i < Count; i++)
        {
            var ticket = Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"], title: $"{_prefix}{i:00}");
            ticket.Fund(i % 5 * 10);
            await _harness.AddAsync(_alice, ticket);
        }
    }

    public async ValueTask DisposeAsync() => await _harness.DisposeAsync();

    [Fact]
    public async Task FirstPage_HasTheRequestedSizeAndTheExactTotal()
    {
        var page = (await PageAsync(new PageRequest(1, 10), new SortRequest())).ShouldBeSuccess();

        page.Items.Count.ShouldBe(10);
        page.TotalCount.ShouldBe(Count);
        page.TotalCountExact.ShouldBeTrue();
        page.Page.ShouldBe(1);
        page.PageSize.ShouldBe(10);
        page.Items.Select(i => i.Title).ShouldBe(Enumerable.Range(0, 10).Select(i => $"{_prefix}{i:00}"));
    }

    [Fact]
    public async Task LastPage_HoldsTheRest()
    {
        var page = (await PageAsync(new PageRequest(3, 10), new SortRequest())).ShouldBeSuccess();

        page.Items.Count.ShouldBe(5);
        page.TotalCount.ShouldBe(Count);
    }

    [Fact]
    public async Task PageBeyondTheEnd_IsEmptyNotAnError()
    {
        var page = (await PageAsync(new PageRequest(9, 10), new SortRequest())).ShouldBeSuccess();

        page.Items.ShouldBeEmpty();
        page.TotalCount.ShouldBe(Count);
    }

    [Fact]
    public async Task VeryLargePageNumber_IsEmptyNotAnOverflow()
    {
        var page = (await PageAsync(new PageRequest(int.MaxValue, 200), new SortRequest())).ShouldBeSuccess();

        page.Items.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0, 20, "page")]
    [InlineData(-1, 20, "page")]
    [InlineData(1, 0, "pageSize")]
    [InlineData(1, 201, "pageSize")]
    public async Task OutOfRangePageParameters_AreRejected(int page, int pageSize, string path)
    {
        var result = await PageAsync(new PageRequest(page, pageSize), new SortRequest());

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.ValidationFailed");
        result.Error.FieldErrors.ShouldHaveSingleItem().Path.ShouldBe(path);
    }

    [Fact]
    public async Task AllProblemsAreReportedTogether()
    {
        var result = await PageAsync(new PageRequest(0, 500), new SortRequest("nope"));

        result.Error.FieldErrors.Select(e => e.Path).ShouldBe(["page", "pageSize", "sort"]);
    }

    [Fact]
    public async Task UnregisteredSortField_IsRejected()
    {
        var result = await PageAsync(new PageRequest(), new SortRequest("budget"));

        result.Error.FieldErrors.ShouldHaveSingleItem().Path.ShouldBe("sort");
    }

    [Fact]
    public async Task Sort_FollowsTheRequestedOrder()
    {
        var page = (await PageAsync(new PageRequest(1, 100), new SortRequest("-amount,title"))).ShouldBeSuccess();

        page.Items.Select(i => i.Amount).ShouldBe(page.Items.Select(i => i.Amount).OrderDescending());
        page.Items.Where(i => i.Amount == 40).Select(i => i.Title).ShouldBe(page.Items.Where(i => i.Amount == 40).Select(i => i.Title).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task TiedSortKeys_PageWithoutDuplicatesOrGaps()
    {
        // 25 条记录只有五种金额：按金额分页，同值的记录要靠追加的 Id 才有固定的先后
        var seen = new List<TicketId>();
        for (var page = 1; page <= 7; page++)
        {
            seen.AddRange((await PageAsync(new PageRequest(page, 4), new SortRequest("amount"))).ShouldBeSuccess().Items.Select(i => i.Id));
        }

        seen.Count.ShouldBe(Count);
        seen.Distinct().Count().ShouldBe(Count);
    }

    [Fact]
    public async Task Capped_UnderTheCap_IsExact()
    {
        var page = (await PageAsync(new PageRequest(1, 10), new SortRequest(), CountMode.CappedAt(30))).ShouldBeSuccess();

        page.TotalCount.ShouldBe(Count);
        page.TotalCountExact.ShouldBeTrue();
    }

    [Fact]
    public async Task Capped_OverTheCap_StopsCountingAndSaysSo()
    {
        var page = (await PageAsync(new PageRequest(1, 5), new SortRequest(), CountMode.CappedAt(10))).ShouldBeSuccess();

        page.TotalCount.ShouldBe(10);
        page.TotalCountExact.ShouldBeFalse();
        page.Items.Count.ShouldBe(5);
    }

    [Fact]
    public async Task Capped_LastPageWithinTheCap_IsStillReachable()
    {
        var page = (await PageAsync(new PageRequest(2, 5), new SortRequest(), CountMode.CappedAt(10))).ShouldBeSuccess();

        page.Items.Count.ShouldBe(5);
        page.TotalCountExact.ShouldBeFalse();
    }

    [Fact]
    public async Task Capped_PageBeyondTheCap_IsRejected()
    {
        var result = await PageAsync(new PageRequest(3, 5), new SortRequest(), CountMode.CappedAt(10));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.PageOutOfRange");
    }

    [Fact]
    public async Task Capped_EmptyBeyondTheEndWhenUnderTheCap_IsNotAnError()
    {
        var page = (await PageAsync(new PageRequest(9, 10), new SortRequest(), CountMode.CappedAt(30))).ShouldBeSuccess();

        page.Items.ShouldBeEmpty();
        page.TotalCountExact.ShouldBeTrue();
    }

    private async Task<Result<PagedResult<TicketListItem>>> PageAsync(PageRequest page, SortRequest sort, CountMode count = default)
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Query.Tickets
            .Where(t => t.Title.StartsWith(_prefix))
            .Select(t => new TicketListItem(t.Id, t.Number, t.Title, t.Amount))
            .ToPagedAsync(page, sort, _sort, count, Ct);
    }
}
