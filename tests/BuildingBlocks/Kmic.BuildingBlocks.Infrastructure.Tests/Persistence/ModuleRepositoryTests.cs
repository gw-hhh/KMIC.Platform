using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

public sealed class ModuleRepositoryTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);
    private static readonly TestIdentity _stranger = TestIdentity.User("乙").InTree(_org).InOrgUnit(_org["总部"]);

    private static readonly Error _reviewNotFound = Error.NotFound("Probe.ReviewNotFound", "评审不存在");

    private readonly ProbeHarness _harness = new(database.ConnectionString, services: s => s
        .AddRepository<ProbeDbContext, Ticket, TicketId>(TicketErrors.NotFound, set => set.Include(t => t.Lines))
        .AddRepository<ProbeDbContext, Review, ReviewId>(_reviewNotFound));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task AddThenGetForUpdate_WithoutInclude_LoadsRoot()
    {
        var review = Review.Create("年度评审");
        await using (var work = _harness.Begin(_alice))
        {
            Reviews(work).Add(review);
            await work.SaveAsync();
        }

        await using var check = _harness.Begin(_alice);
        var loaded = (await Reviews(check).GetForUpdateAsync(review.Id, expectedVersion: 0, Ct)).ShouldBeSuccess();

        loaded.Title.ShouldBe("年度评审");
    }

    [Fact]
    public async Task GetForUpdate_WithInclude_LoadsChildren()
    {
        var ticket = NewTicket();
        ticket.AddLine("第一行");
        ticket.AddLine("第二行");
        await _harness.AddAsync(_alice, ticket);

        await using var work = _harness.Begin(_alice);
        var loaded = (await Tickets(work).GetForUpdateAsync(ticket.Id, null, Ct)).ShouldBeSuccess();

        loaded.Lines.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetForUpdate_Missing_ReturnsRegisteredError()
    {
        await using var work = _harness.Begin(_alice);

        (await Reviews(work).GetForUpdateAsync(ReviewId.New(), null, Ct)).ShouldBeFailure(_reviewNotFound);
    }

    [Fact]
    public async Task GetForUpdate_StaleExpectedVersion_Conflict()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        await using var work = _harness.Begin(_alice);

        (await Tickets(work).GetForUpdateAsync(ticket.Id, expectedVersion: 5, Ct)).ShouldBeFailure(CommonErrors.VersionConflict);
    }

    [Fact]
    public async Task GetForUpdate_OutOfScope_NotFoundAndSecurityEvent()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        await using var work = _harness.Begin(_stranger);
        (await Tickets(work).GetForUpdateAsync(ticket.Id, null, Ct)).ShouldBeFailure(TicketErrors.NotFound);

        _harness.SecurityEvents.Events.ShouldHaveSingleItem().Reason.ShouldBe("OutOfDataScope");
    }

    [Fact]
    public void AddRepository_Twice_Throws()
    {
        var services = new ServiceCollection().AddRepository<ProbeDbContext, Review, ReviewId>(_reviewNotFound);

        Should.Throw<InvalidOperationException>(() => services.AddRepository<ProbeDbContext, Review, ReviewId>(_reviewNotFound))
            .Message.ShouldContain(nameof(Review));
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static IRepository<Ticket, TicketId> Tickets(ProbeWork work) => work.Services.GetRequiredService<IRepository<Ticket, TicketId>>();

    private static IRepository<Review, ReviewId> Reviews(ProbeWork work) => work.Services.GetRequiredService<IRepository<Review, ReviewId>>();

    private static Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);
}
