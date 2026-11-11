using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

/// <summary>
/// 仓储基类：加载完整聚合、版本比较、范围外的真实原因、加锁加载、恢复、原子条件更新
/// </summary>
public sealed class RepositoryTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);
    private static readonly TestIdentity _stranger = TestIdentity.User("乙").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetForUpdate_LoadsWholeAggregate()
    {
        var ticket = NewTicket();
        ticket.AddLine("第一行");
        ticket.AddLine("第二行");
        await _harness.AddAsync(_alice, ticket);

        await using var work = _harness.Begin(_alice);
        var loaded = (await work.Tickets.GetForUpdateAsync(ticket.Id, expectedVersion: 0, Ct)).ShouldBeSuccess();

        loaded.Lines.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetForUpdate_StaleExpectedVersion_ConflictWithCurrentVersion()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        await EditAsync(ticket.Id, t => t.Rename("改过一次"));

        await using var work = _harness.Begin(_alice);
        var result = await work.Tickets.GetForUpdateAsync(ticket.Id, expectedVersion: 0, Ct);

        var error = result.ShouldBeFailure(CommonErrors.VersionConflict);
        error.Metadata[ErrorMetadataKeys.CurrentVersion].ShouldBe(1L);
    }

    [Fact]
    public async Task GetForUpdate_OutOfScope_NotFoundAndSecurityEvent()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        await using var work = _harness.Begin(_stranger);
        (await work.Tickets.GetForUpdateAsync(ticket.Id, null, Ct)).ShouldBeFailure(TicketErrors.NotFound);

        var recorded = _harness.SecurityEvents.Events.ShouldHaveSingleItem();
        recorded.Reason.ShouldBe("OutOfDataScope");
        recorded.ResourceType.ShouldBe(nameof(Ticket));
        recorded.ResourceId.ShouldBe(ticket.Id.Value);
        recorded.UserId.ShouldBe(_stranger.UserId.Value);
    }

    [Fact]
    public async Task GetForUpdate_MissingOrDeleted_NotFoundWithoutSecurityEvent()
    {
        var deleted = await _harness.AddAsync(_alice, NewTicket());
        await RemoveAsync(deleted.Id);

        await using var work = _harness.Begin(_alice);
        (await work.Tickets.GetForUpdateAsync(TicketId.New(), null, Ct)).ShouldBeFailure(TicketErrors.NotFound);
        (await work.Tickets.GetForUpdateAsync(deleted.Id, null, Ct)).ShouldBeFailure(TicketErrors.NotFound);

        _harness.SecurityEvents.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task Restore_DeletedAggregate_VisibleAgainAndVersionIncremented()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        await RemoveAsync(ticket.Id);

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Tickets.RestoreAsync(ticket.Id, expectedVersion: 1, Ct)).ShouldBeSuccess();
            await work.SaveAsync();
        }

        await using var check = _harness.Begin(_alice);
        (await check.Query.Tickets.Where(t => t.Id == ticket.Id).Select(t => t.ConcurrencyVersion).SingleAsync(Ct)).ShouldBe(2);
    }

    [Fact]
    public async Task GetForUpdateLocked_OutsideTransaction_Throws()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        await using var work = _harness.Begin(_alice);

        await Should.ThrowAsync<InvalidOperationException>(() => work.Tickets.GetForUpdateLockedAsync(ticket.Id, null, Ct));
    }

    [Fact]
    public async Task GetForUpdateLocked_HoldsRowLockUntilCommit()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());

        await using var holder = _harness.Begin(_alice);
        await using var holding = await holder.Db.Database.BeginTransactionAsync(Ct);
        (await holder.Tickets.GetForUpdateLockedAsync(ticket.Id, null, Ct)).ShouldBeSuccess();

        await using var waiter = _harness.Begin(_alice);
        await using var waiting = await waiter.Db.Database.BeginTransactionAsync(Ct);
        await waiter.Db.Database.ExecuteSqlAsync($"SET LOCAL lock_timeout = '300ms'", Ct);

        var error = await Should.ThrowAsync<PostgresException>(() => waiter.Tickets.GetForUpdateLockedAsync(ticket.Id, null, Ct));
        error.SqlState.ShouldBe(PostgresErrorCodes.LockNotAvailable);
    }

    [Fact]
    public async Task UpdateWhere_ConcurrentDeductions_NeverOverdraw()
    {
        var ticket = NewTicket();
        ticket.Fund(100m);
        await _harness.AddAsync(_alice, ticket);

        // 两个事务同时扣 70：后者等前者提交后重新判断条件，扣不成
        var results = await Task.WhenAll(DeductAsync(ticket.Id, 70m), DeductAsync(ticket.Id, 70m));

        results.Count(deducted => deducted).ShouldBe(1);
        await using var check = _harness.BeginAsSystem();
        var row = await check.Query.Tickets
            .Where(t => t.Id == ticket.Id)
            .Select(t => new { t.Amount, t.ConcurrencyVersion, UpdatedBy = EF.Property<UserId?>(t, AuditColumns.UpdatedBy) })
            .SingleAsync(Ct);
        row.Amount.ShouldBe(30m);
        row.ConcurrencyVersion.ShouldBe(1);
        row.UpdatedBy.ShouldBe(_alice.UserId);
    }

    [Fact]
    public async Task UpdateWhere_OutsideTransaction_Throws()
    {
        var ticket = await _harness.AddAsync(_alice, NewTicket());
        await using var work = _harness.Begin(_alice);

        await Should.ThrowAsync<InvalidOperationException>(() => work.Tickets.TryDeductAsync(ticket.Id, 1m, Ct));
    }

    [Fact]
    public async Task UpdateWhere_OutOfScope_UpdatesNothing()
    {
        var ticket = NewTicket();
        ticket.Fund(100m);
        await _harness.AddAsync(_alice, ticket);

        await using var work = _harness.Begin(_stranger.WithRole(r => r.Scope(ScopeLevel.Self)));
        await using var transaction = await work.Db.Database.BeginTransactionAsync(Ct);

        (await work.Tickets.TryDeductAsync(ticket.Id, 10m, Ct)).ShouldBeFalse();
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static Ticket NewTicket() => Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);

    private async Task<bool> DeductAsync(TicketId id, decimal amount)
    {
        await using var work = _harness.Begin(_alice);
        await using var transaction = await work.Db.Database.BeginTransactionAsync(Ct);
        var deducted = await work.Tickets.TryDeductAsync(id, amount, Ct);
        await transaction.CommitAsync(Ct);
        return deducted;
    }

    private async Task EditAsync(TicketId id, Action<Ticket> edit)
    {
        await using var work = _harness.Begin(_alice);
        edit((await work.Tickets.GetForUpdateAsync(id, null, Ct)).ShouldBeSuccess());
        await work.SaveAsync();
    }

    private async Task RemoveAsync(TicketId id)
    {
        await using var work = _harness.Begin(_alice);
        work.Tickets.Remove((await work.Tickets.GetForUpdateAsync(id, null, Ct)).ShouldBeSuccess());
        await work.SaveAsync();
    }
}
