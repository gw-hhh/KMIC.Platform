using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

public sealed class OutboxWriteTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SnapshotEvent_WrittenWithCommittedVersion()
    {
        var ticket = await NewTicketAsync();

        (await ShareAsync(ticket.Id, "新标题")).ShouldBeSuccess();

        var row = (await OutboxForAsync(ticket.Id)).ShouldHaveSingleItem();
        row.MessageType.ShouldBe("BuildingBlocks.TicketChanged");
        row.SchemaVersion.ShouldBe(1);
        row.Kind.ShouldBe((short)0);
        row.SourceVersion.ShouldBe(1);
        row.OrderingKey.ShouldBe(ticket.Id.Value.ToString());
        row.OccurredAt.ShouldBe(_harness.Time.GetUtcNow());
        row.Status.ShouldBe(MessageStatus.Pending);
        row.Attempts.ShouldBe(0);
        row.NextAttemptAt.ShouldNotBe(default);
        Read<TicketChanged>(row).Ticket.Title.ShouldBe("新标题");

        var headers = JsonDocument.Parse(row.Headers).RootElement;
        headers.GetProperty("sourceModule").GetString().ShouldBe("BuildingBlocks");
        headers.GetProperty("initiatorUserId").GetGuid().ShouldBe(_alice.UserId.Value);
    }

    [Fact]
    public async Task SameAggregateChangedTwice_KeepsLastSnapshot()
    {
        var ticket = await NewTicketAsync();

        (await ShareAsync(ticket.Id, "第一次", "第二次")).ShouldBeSuccess();

        Read<TicketChanged>((await OutboxForAsync(ticket.Id)).ShouldHaveSingleItem()).Ticket.Title.ShouldBe("第二次");
    }

    [Fact]
    public async Task CommandFails_NothingWrittenAndNothingCarriedOver()
    {
        var ticket = await NewTicketAsync();

        await using var work = _harness.Begin(_alice);
        (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["不该发出"], Fail: true), Ct)).ShouldBeFailure(TicketErrors.Rejected);

        // 同一个作用域里接着成功的命令，只写它自己发布的消息
        (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["应该发出"]), Ct)).ShouldBeSuccess();
        Read<TicketChanged>((await OutboxForAsync(ticket.Id)).ShouldHaveSingleItem()).Ticket.Title.ShouldBe("应该发出");
    }

    [Fact]
    public async Task DomainEventHandler_PublishedEventWritten()
    {
        var ticket = await NewTicketAsync();

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new CloseTicketCommand(ticket.Id), Ct)).ShouldBeSuccess();
        }

        await using var check = _harness.BeginAsSystem();
        var notices = await check.Db.Set<OutboxMessage>().AsNoTracking().Where(m => m.MessageType == "BuildingBlocks.TicketClosedNotice").ToListAsync(Ct);
        var row = notices.Where(n => Read<TicketClosedNotice>(n).TicketId == ticket.Id.Value).ShouldHaveSingleItem();
        row.SourceId.ShouldBeNull();
        row.SourceVersion.ShouldBeNull();
    }

    [Fact]
    public async Task PublishOutsideCommand_Throws()
    {
        await using var scope = _harness.Services.GetRequiredService<IServiceScopeFactory>().CreateAsyncScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        Should.Throw<InvalidOperationException>(() => publisher.Add(new TicketClosedNotice(Guid.CreateVersion7())));
    }

    [Fact]
    public async Task PayloadOverLimit_CommandThrowsAndRollsBack()
    {
        await using var harness = new ProbeHarness(database.ConnectionString, messaging: o => o.MaxPayloadBytes = 64);
        var ticket = await harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

        await using (var work = harness.Begin(_alice))
        {
            await Should.ThrowAsync<InvalidOperationException>(async () =>
                await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, [new string('长', 40)]), Ct));
        }

        (await OutboxForAsync(ticket.Id)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Notify_SentOnCommitOnly()
    {
        var ticket = await NewTicketAsync();
        await using var listener = new NpgsqlConnection(database.ConnectionString);
        await listener.OpenAsync(Ct);
        var received = new List<string>();
        listener.Notification += (_, e) => received.Add(e.Payload);
        await using (var listen = new NpgsqlCommand($"LISTEN {MessagingChannels.Outbox}", listener))
        {
            await listen.ExecuteNonQueryAsync(Ct);
        }

        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["失败"], Fail: true), Ct)).ShouldBeFailure(TicketErrors.Rejected);
        }

        WaitForNotification(listener, TimeSpan.FromMilliseconds(300)).ShouldBeFalse();

        (await ShareAsync(ticket.Id, "成功")).ShouldBeSuccess();

        WaitForNotification(listener, TimeSpan.FromSeconds(5)).ShouldBeTrue();
        received.ShouldBe(["BuildingBlocks"]);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private static T Read<T>(OutboxMessage row)
        where T : class =>
        JsonSerializer.Deserialize<T>(row.Payload, KmicJsonOptions.Stored).ShouldNotBeNull();

    [SuppressMessage("Usage", "CA1849", Justification = "Windows 上 Npgsql 的 WaitAsync 不响应超时，和监听循环一样用同步等待")]
    [SuppressMessage("Usage", "MA0042", Justification = "同上")]
    private static bool WaitForNotification(NpgsqlConnection listener, TimeSpan timeout) => listener.Wait(timeout);

    private Task<Ticket> NewTicketAsync() => _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

    private async Task<Result> ShareAsync(TicketId id, params string[] titles)
    {
        await using var work = _harness.Begin(_alice);
        return await work.Sender.SendAsync(new ShareTicketCommand(id, titles), Ct);
    }

    private async Task<List<OutboxMessage>> OutboxForAsync(TicketId id)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<OutboxMessage>().AsNoTracking().Where(m => m.SourceId == id.Value).ToListAsync(Ct);
    }
}
