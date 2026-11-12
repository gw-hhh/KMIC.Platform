using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Querying;

public sealed class EntityFinderTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);
    private static readonly TestIdentity _stranger = TestIdentity.User("乙").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task InScope_ReturnsTheEntityWithItsIncludes()
    {
        var ticket = Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);
        ticket.AddLine("第一行");
        await _harness.AddAsync(_alice, ticket);

        await using var work = _harness.Begin(_alice);
        var found = await Finder(work).FindByIdOrNotFoundAsync(work.Query.Tickets.Include(t => t.Lines), ticket.Id, TicketErrors.NotFound, Ct);

        found.ShouldBeSuccess().Lines.Count.ShouldBe(1);
        _harness.SecurityEvents.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task OutOfScope_NotFoundAndSecurityEvent()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

        await using var work = _harness.Begin(_stranger);
        (await Finder(work).FindByIdOrNotFoundAsync(work.Query.Tickets, ticket.Id, TicketErrors.NotFound, Ct)).ShouldBeFailure(TicketErrors.NotFound);

        var recorded = _harness.SecurityEvents.Events.ShouldHaveSingleItem();
        recorded.Type.ShouldBe("AuthorizationDenied");
        recorded.Reason.ShouldBe("OutOfDataScope");
        recorded.ResourceType.ShouldBe(nameof(Ticket));
        recorded.ResourceId.ShouldBe(ticket.Id.Value);
        recorded.UserId.ShouldBe(_stranger.UserId.Value);
    }

    [Fact]
    public async Task MissingOrDeleted_NotFoundWithoutSecurityEvent()
    {
        var deleted = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await using (var removal = _harness.Begin(_alice))
        {
            removal.Tickets.Remove((await removal.Tickets.GetForUpdateAsync(deleted.Id, null, Ct)).ShouldBeSuccess());
            await removal.SaveAsync();
        }

        await using var work = _harness.Begin(_alice);
        (await Finder(work).FindByIdOrNotFoundAsync(work.Query.Tickets, TicketId.New(), TicketErrors.NotFound, Ct)).ShouldBeFailure(TicketErrors.NotFound);
        (await Finder(work).FindByIdOrNotFoundAsync(work.Query.Tickets, deleted.Id, TicketErrors.NotFound, Ct)).ShouldBeFailure(TicketErrors.NotFound);

        _harness.SecurityEvents.Events.ShouldBeEmpty();
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static IEntityFinder Finder(ProbeWork work) => work.Services.GetRequiredService<IEntityFinder>();
}
