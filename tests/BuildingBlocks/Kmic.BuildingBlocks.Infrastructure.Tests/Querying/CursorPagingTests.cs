using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Querying;

/// <summary>
/// 游标分页在真实 PostgreSQL 上执行：25 张单据，一种按创建时间排序，一种排序键全部相同（标题长度），只能靠 ID 定先后
/// </summary>
public sealed class CursorPagingTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const int Count = 25;

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);
    private static readonly HmacCursorProtector _protector = new(Options.Create(new CursorOptions()));

    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly string _prefix = $"C{Guid.CreateVersion7():N}-";

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WalkingAllThePages_ByCreationTime_VisitsEveryRowOnce_InTheFixedOrder(bool descending)
    {
        var seen = await WalkAsync(7, descending, tied: false);

        seen.Count.ShouldBe(Count);
        seen.Select(r => r.Id).Distinct().Count().ShouldBe(Count);
        var expected = descending ? seen.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id.Value) : seen.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id.Value);
        seen.ShouldBe(expected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WalkingAllThePages_WhenEveryKeyIsTheSame_FallsBackToTheIdAlone(bool descending)
    {
        var seen = await WalkAsync(4, descending, tied: true);

        seen.Count.ShouldBe(Count);
        seen.Select(r => r.Id).Distinct().Count().ShouldBe(Count);
        seen.Select(r => r.Id.Value).ShouldBe(descending ? seen.Select(r => r.Id.Value).OrderByDescending(i => i) : seen.Select(r => r.Id.Value).Order());
    }

    private async Task<List<Row>> WalkAsync(int limit, bool descending, bool tied)
    {
        var seen = new List<Row>();
        string? after = null;
        do
        {
            var page = (await PageAsync(new CursorRequest(after, limit), descending, tied)).ShouldBeSuccess();
            page.Items.Count.ShouldBeLessThanOrEqualTo(limit);
            seen.AddRange(page.Items);
            after = page.NextCursor;
        }
        while (after is not null);

        return seen;
    }

    [Fact]
    public async Task APageThatEndsExactlyAtTheLastRow_HasNoNextCursor()
    {
        var page = (await PageAsync(new CursorRequest(null, 25), descending: true)).ShouldBeSuccess();

        page.Items.Count.ShouldBe(Count);
        page.NextCursor.ShouldBeNull();
    }

    [Fact]
    public async Task RowsInsertedWhileWalking_DoNotShiftTheRemainingPages()
    {
        var first = (await PageAsync(new CursorRequest(null, 10), descending: true)).ShouldBeSuccess();
        var ticket = Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"], title: $"{_prefix}late");
        ticket.Fund(990);
        await _harness.AddAsync(_alice, ticket);

        var second = (await PageAsync(new CursorRequest(first.NextCursor, 10), descending: true)).ShouldBeSuccess();
        var third = (await PageAsync(new CursorRequest(second.NextCursor, 10), descending: true)).ShouldBeSuccess();

        var all = first.Items.Concat(second.Items).Concat(third.Items).ToList();
        all.Count.ShouldBe(Count);
        all.Select(r => r.Id).Distinct().Count().ShouldBe(Count);
        third.NextCursor.ShouldBeNull();
    }

    [Fact]
    public async Task ATamperedOrForeignCursor_IsRefusedWithAFieldError()
    {
        var first = (await PageAsync(new CursorRequest(null, 5), descending: true)).ShouldBeSuccess();

        var tampered = await PageAsync(new CursorRequest(first.NextCursor + "x", 5), descending: true);
        var nonsense = await PageAsync(new CursorRequest("nonsense", 5), descending: true);

        foreach (var result in new[] { tampered, nonsense })
        {
            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Common.ValidationFailed");
            result.Error.FieldErrors.ShouldHaveSingleItem().Path.ShouldBe("after");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task ALimitOutsideTheRange_IsRefusedWithAFieldError(int limit)
    {
        var result = await PageAsync(new CursorRequest(null, limit), descending: true);

        result.IsFailure.ShouldBeTrue();
        result.Error.FieldErrors.ShouldHaveSingleItem().Path.ShouldBe("limit");
    }

    private async Task<Result<CursorPage<Row>>> PageAsync(CursorRequest request, bool descending, bool tied = false)
    {
        await using var work = _harness.BeginAsSystem();
        var rows = work.Query.Tickets
            .Where(t => t.Title.StartsWith(_prefix))
            .Select(t => new Row(t.Id, t.Title, EF.Property<DateTimeOffset>(t, "CreatedAt"), t.Title.Length));
        return tied
            ? await rows.ToCursorPageAsync(request, _protector, r => r.TitleLength, r => r.Id, descending, Ct)
            : await rows.ToCursorPageAsync(request, _protector, r => r.CreatedAt, r => r.Id, descending, Ct);
    }

    internal sealed record Row(TicketId Id, string Title, DateTimeOffset CreatedAt, int TitleLength);
}
