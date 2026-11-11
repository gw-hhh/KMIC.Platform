using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Pipeline;

/// <summary>
/// 命令事务和异常转换
/// </summary>
public sealed class TransactionBehaviorTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Success_CommitsAndReturnsSavedVersion()
    {
        var created = (await OpenAsync(TicketNumbers.Next())).ShouldBeSuccess();

        await using var work = _harness.Begin(_alice);
        var renamed = (await work.Sender.SendAsync(new RenameTicketCommand(created.Id, created.ConcurrencyVersion, "改名"), Ct)).ShouldBeSuccess();

        // 处理器填的是保存前的 0，返回的是提交后的 1
        renamed.ConcurrencyVersion.ShouldBe(1);
        (await VersionOfAsync(created.Id)).ShouldBe(1);
    }

    [Fact]
    public async Task HandlerFailure_RollsBackEverything()
    {
        var number = TicketNumbers.Next();

        (await OpenAsync(number, OpenTicketCommand.RejectedTitle)).ShouldBeFailure(TicketErrors.Rejected);

        await using var check = _harness.BeginAsSystem();
        (await check.Query.Tickets.AnyAsync(t => t.Number == number, Ct)).ShouldBeFalse();
    }

    [Fact]
    public async Task DomainEvent_HandledInSameTransaction_ThenAfterCommitRuns()
    {
        var created = (await OpenAsync(TicketNumbers.Next())).ShouldBeSuccess();

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new CloseTicketCommand(created.Id), Ct)).ShouldBeSuccess();
        }

        await using var check = _harness.BeginAsSystem();
        var ticket = await check.Query.Tickets.Include(t => t.Lines).SingleAsync(t => t.Id == created.Id, Ct);
        ticket.Status.ShouldBe(TicketStatus.Closed);
        ticket.Lines.ShouldHaveSingleItem().Text.ShouldBe("已关闭");
        _harness.Hooks.AfterCommitRuns.ShouldBe(1);
    }

    [Fact]
    public async Task DomainEventFailure_RollsBackCommandAndSkipsAfterCommit()
    {
        var created = (await OpenAsync(TicketNumbers.Next())).ShouldBeSuccess();
        _harness.Hooks.FailClosedEvent = true;

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new CloseTicketCommand(created.Id), Ct)).ShouldBeFailure(TicketErrors.Rejected);
        }

        await using var check = _harness.BeginAsSystem();
        (await check.Query.Tickets.SingleAsync(t => t.Id == created.Id, Ct)).Status.ShouldBe(TicketStatus.Open);
        _harness.Hooks.AfterCommitRuns.ShouldBe(0);
    }

    [Fact]
    public async Task UniqueViolation_TranslatedToMappedError()
    {
        var number = TicketNumbers.Next();
        (await OpenAsync(number)).ShouldBeSuccess();

        (await OpenAsync(number.ToUpperInvariant())).ShouldBeFailure(TicketErrors.NumberDuplicated);
    }

    [Fact]
    public async Task ConcurrentModification_VersionConflictWithCurrentVersion()
    {
        var created = (await OpenAsync(TicketNumbers.Next())).ShouldBeSuccess();

        // 处理器读取之后、保存之前，另一个连接改了这一行
        _harness.Hooks.OnBeforeReturn(async ct =>
        {
            await using var command = database.DataSource.CreateCommand(
                $"""UPDATE "Probe"."Ticket" SET "ConcurrencyVersion" = "ConcurrencyVersion" + 5 WHERE "Id" = '{created.Id.Value}'""");
            await command.ExecuteNonQueryAsync(ct);
        });

        await using var work = _harness.Begin(_alice);
        var result = await work.Sender.SendAsync(new RenameTicketCommand(created.Id, 0, "后保存"), Ct);

        result.ShouldBeFailure(CommonErrors.VersionConflict).Metadata[ErrorMetadataKeys.CurrentVersion].ShouldBe(5L);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task<Result<CreatedResponse<TicketId>>> OpenAsync(string number, string title = "测试单据")
    {
        await using var work = _harness.Begin(_alice);
        work.Request.IdempotencyKey = Guid.CreateVersion7().ToString();
        return await work.Sender.SendAsync(new OpenTicketCommand(number, title), Ct);
    }

    private async Task<long> VersionOfAsync(TicketId id)
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Query.Tickets.Where(t => t.Id == id).Select(t => t.ConcurrencyVersion).SingleAsync(Ct);
    }
}
