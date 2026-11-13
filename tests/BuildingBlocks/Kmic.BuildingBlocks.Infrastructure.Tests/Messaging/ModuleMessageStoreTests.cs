using System.Globalization;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// Operations 管理消息用的存储：积压汇总、死信列表与内容、被挡住的键、重试、放弃，在真实 PostgreSQL 上执行。
/// 同一个类里的测试共用一个库，所以每个测试用自己的消息类型、订阅和顺序键，互不干扰
/// </summary>
public sealed class ModuleMessageStoreTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Module = "Echo";
    private const short Pending = 0;
    private const short Processing = 1;
    private const short Done = 2;
    private const short Dead = 3;
    private const short Abandoned = 4;

    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly string _unique = Guid.CreateVersion7().ToString("N");
    private long _seq = 1;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IModuleMessageStore Store => _harness.Services.GetRequiredService<IModuleMessageStores>().Find(Module).ShouldNotBeNull();

    private ModuleFrameworkTables Tables => _harness.Services.GetRequiredService<ModuleFrameworkTables>();

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    [Fact]
    public void EveryModuleWithADatabaseHasAStore()
    {
        var stores = _harness.Services.GetRequiredService<IModuleMessageStores>();

        var names = stores.All.Select(s => s.Module).ToList();
        names.Contains(Module, StringComparer.Ordinal).ShouldBeTrue();
        names.ShouldBe(names.Order(StringComparer.Ordinal));
        stores.Find("NoSuchModule").ShouldBeNull();
    }

    [Fact]
    public async Task Backlog_CountsEachBoxAndSubscription_AndOnlyOrderedSubscriptionsHaveBlockedKeys()
    {
        var before = await Store.GetBacklogAsync(Ct);
        var plain = "Echo.Plain" + _unique;
        var ordered = ProbeHarness.MirrorSubscription;
        var orderedBefore = before.Subscriptions.SingleOrDefault(s => string.Equals(s.Subscription, ordered, StringComparison.Ordinal));
        await InsertOutboxAsync(Pending, occurred: Hours(-2));
        await InsertOutboxAsync(Processing);
        await InsertOutboxAsync(Dead);
        await InsertOutboxAsync(Done);
        await InsertInboxAsync(plain, Pending, received: Hours(-3));
        await InsertInboxAsync(plain, Dead, key: "k1");
        await InsertInboxAsync(plain, Dead, key: "k2");
        await InsertInboxAsync(plain, Abandoned);
        await InsertInboxAsync(ordered, Dead, key: "blocked-" + _unique);

        var after = await Store.GetBacklogAsync(Ct);

        after.Outbox.Pending.ShouldBe(before.Outbox.Pending + 2);
        after.Outbox.Dead.ShouldBe(before.Outbox.Dead + 1);
        after.Outbox.OldestAge.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromHours(1.9));
        var plainBacklog = after.Subscriptions.Single(s => string.Equals(s.Subscription, plain, StringComparison.Ordinal));
        plainBacklog.Pending.ShouldBe(1);
        plainBacklog.OldestAge.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromHours(2.9));
        plainBacklog.Dead.ShouldBe(2);
        plainBacklog.Abandoned.ShouldBe(1);
        plainBacklog.BlockedKeys.ShouldBe(0, "没有声明为有序的订阅里，死信不挡别的消息");
        var orderedAfter = after.Subscriptions.Single(s => string.Equals(s.Subscription, ordered, StringComparison.Ordinal));
        orderedAfter.BlockedKeys.ShouldBe((orderedBefore?.BlockedKeys ?? 0) + 1);
        after.Subscriptions.Select(s => s.Subscription).ShouldBe(after.Subscriptions.Select(s => s.Subscription).Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ListDead_NewestFirst_BothBoxes_OnlyDeadRows_PagedByPosition_AndFiltered()
    {
        var type = "Test.List" + _unique;
        var subscription = "Echo.ListSub" + _unique;
        var now = DateTimeOffset.Parse("2026-11-13T08:00:00Z", CultureInfo.InvariantCulture);
        var o1 = await InsertOutboxAsync(Dead, type: type, occurred: now.AddMinutes(-50));
        var i1 = await InsertInboxAsync(subscription, Dead, type: type, occurred: now.AddMinutes(-40));
        var o2 = await InsertOutboxAsync(Dead, type: type, occurred: now.AddMinutes(-30));
        var i2 = await InsertInboxAsync(subscription, Dead, type: type, occurred: now.AddMinutes(-20));
        await InsertOutboxAsync(Pending, type: type, occurred: now.AddMinutes(-10));
        await InsertInboxAsync(subscription, Abandoned, type: type, occurred: now.AddMinutes(-5));
        await InsertInboxAsync(subscription, Done, type: type, occurred: now);

        var all = await Store.ListDeadAsync(new DeadMessageFilter(MessageType: type), null, 100, Ct);
        var inboxOnly = await Store.ListDeadAsync(new DeadMessageFilter(MessageBox.Inbox, MessageType: type), null, 100, Ct);
        var outboxOnly = await Store.ListDeadAsync(new DeadMessageFilter(MessageBox.Outbox, MessageType: type), null, 100, Ct);
        var bySubscription = await Store.ListDeadAsync(new DeadMessageFilter(Subscription: subscription, MessageType: type), null, 100, Ct);
        var seen = new List<Guid>();
        MessagePosition? after = null;
        while (true)
        {
            var page = await Store.ListDeadAsync(new DeadMessageFilter(MessageType: type), after, 3, Ct);
            seen.AddRange(page.Select(m => m.Id));
            if (page.Count < 3)
            {
                break;
            }

            after = new MessagePosition(page[^1].OccurredAt, page[^1].Id);
        }

        all.Select(m => m.Id).ShouldBe([i2, o2, i1, o1]);
        all.Select(m => m.Box).ShouldBe([MessageBox.Inbox, MessageBox.Outbox, MessageBox.Inbox, MessageBox.Outbox]);
        all.ShouldAllBe(m => string.Equals(m.Module, Module, StringComparison.Ordinal));
        all.Single(m => m.Id == i1).Subscription.ShouldBe(subscription);
        all.Single(m => m.Id == o1).Subscription.ShouldBeNull();
        inboxOnly.Select(m => m.Id).ShouldBe([i2, i1]);
        outboxOnly.Select(m => m.Id).ShouldBe([o2, o1]);
        bySubscription.Select(m => m.Id).ShouldBe([i2, i1], "给了订阅就只看收件箱");
        seen.ShouldBe([i2, o2, i1, o1]);
    }

    [Fact]
    public async Task Get_ShowsPayloadAndHeaders_AndTheState_OfAnyRow()
    {
        var subscription = "Echo.GetSub" + _unique;
        var dead = await InsertInboxAsync(subscription, Dead, error: "处理失败", payload: """{"x":1}""", headers: """{"h":"v"}""");
        var abandoned = await InsertInboxAsync(subscription, Abandoned);
        var pending = await InsertInboxAsync(subscription, Pending);
        var outboxDead = await InsertOutboxAsync(Dead, payload: """{"y":2}""");
        var outboxPending = await InsertOutboxAsync(Pending);

        var deadDetail = (await Store.GetAsync(MessageBox.Inbox, dead, Ct)).ShouldNotBeNull();
        var abandonedDetail = (await Store.GetAsync(MessageBox.Inbox, abandoned, Ct)).ShouldNotBeNull();
        var outboxDetail = (await Store.GetAsync(MessageBox.Outbox, outboxDead, Ct)).ShouldNotBeNull();

        deadDetail.State.ShouldBe(DeadMessageState.Dead);
        deadDetail.Message.LastError.ShouldBe("处理失败");
        deadDetail.Payload.ShouldContain("\"x\"");
        deadDetail.Headers.ShouldContain("\"h\"");
        deadDetail.Message.Subscription.ShouldBe(subscription);
        abandonedDetail.State.ShouldBe(DeadMessageState.Abandoned);
        outboxDetail.Message.Box.ShouldBe(MessageBox.Outbox);
        outboxDetail.Payload.ShouldContain("\"y\"");
        (await Store.GetAsync(MessageBox.Inbox, pending, Ct)).ShouldNotBeNull().State.ShouldBe(DeadMessageState.Other);
        (await Store.GetAsync(MessageBox.Outbox, outboxPending, Ct)).ShouldNotBeNull().State.ShouldBe(DeadMessageState.Other);
        (await Store.GetAsync(MessageBox.Inbox, outboxDead, Ct)).ShouldBeNull("ID 在另一个箱里");
        (await Store.GetAsync(MessageBox.Inbox, Guid.CreateVersion7(), Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task BlockedKeys_AreTheOrderedKeysWithADeadRow_WithWhatIsWaitingBehindTheFirstOne()
    {
        var ordered = ProbeHarness.MirrorSubscription;
        var a = "key-a-" + _unique;
        var b = "key-b-" + _unique;
        var c = "key-c-" + _unique;
        var firstDead = await InsertInboxAsync(ordered, Dead, key: a, error: "第一次出错");
        await InsertInboxAsync(ordered, Pending, key: a);
        await InsertInboxAsync(ordered, Pending, key: a);
        await InsertInboxAsync(ordered, Processing, key: a);
        await InsertInboxAsync(ordered, Done, key: a);
        await InsertInboxAsync(ordered, Dead, key: b);
        await InsertInboxAsync(ordered, Dead, key: b);
        await InsertInboxAsync(ordered, Pending, key: c);
        var unordered = "Echo.Unordered" + _unique;
        await InsertInboxAsync(unordered, Dead, key: a);

        var keys = (await Store.ListBlockedKeysAsync(Ct)).Where(k => k.OrderingKey.EndsWith(_unique, StringComparison.Ordinal)).ToList();

        keys.Select(k => k.OrderingKey).ShouldBe([a, b]);
        var blockedA = keys.Single(k => string.Equals(k.OrderingKey, a, StringComparison.Ordinal));
        blockedA.Subscription.ShouldBe(ordered);
        blockedA.DeadCount.ShouldBe(1);
        blockedA.Waiting.ShouldBe(3);
        blockedA.FirstDeadId.ShouldBe(firstDead);
        blockedA.LastError.ShouldBe("第一次出错");
        keys.Single(k => string.Equals(k.OrderingKey, b, StringComparison.Ordinal)).DeadCount.ShouldBe(2);
        keys.Single(k => string.Equals(k.OrderingKey, b, StringComparison.Ordinal)).Waiting.ShouldBe(0);
    }

    [Theory]
    [InlineData(MessageBox.Inbox)]
    [InlineData(MessageBox.Outbox)]
    public async Task Retry_PutsDeadRowsBackToPending_AndLeavesTheOthersAlone(MessageBox box)
    {
        var dead = await InsertAsync(box, Dead, attempts: 10);
        var pending = await InsertAsync(box, Pending, attempts: 2);
        var done = await InsertAsync(box, Done, attempts: 1);
        var unknown = Guid.CreateVersion7();

        var results = await Store.RetryAsync(box, [dead, pending, done, unknown, dead], Ct);

        results.Select(r => r.Id).ShouldBe([dead, pending, done, unknown, dead]);
        results.Select(r => r.Outcome).ShouldBe([MessageActionOutcome.Done, MessageActionOutcome.WrongState, MessageActionOutcome.WrongState, MessageActionOutcome.NotFound, MessageActionOutcome.Done]);
        var row = await RowAsync(box, dead);
        row.Status.ShouldBe(Pending);
        row.Attempts.ShouldBe(0);
        row.LeaseToken.ShouldBeNull();
        row.NextAttemptAt.ShouldBeLessThanOrEqualTo(TimeProvider.System.GetUtcNow().AddSeconds(5));
        (await RowAsync(box, pending)).Attempts.ShouldBe(2);
        (await RowAsync(box, done)).Status.ShouldBe(Done);
        (await Store.RetryAsync(box, [dead], Ct)).Single().Outcome.ShouldBe(MessageActionOutcome.WrongState, "已经不是死信了，不会再改一次");
    }

    [Fact]
    public async Task Abandon_MarksDeadInboxRows_RecordsTheReasonAndTheOldError_AndNeverTouchesTheRest()
    {
        var subscription = "Echo.AbandonSub" + _unique;
        var dead = await InsertInboxAsync(subscription, Dead, error: "原来的错误");
        var pending = await InsertInboxAsync(subscription, Pending);
        var alreadyAbandoned = await InsertInboxAsync(subscription, Abandoned);
        var outboxDead = await InsertOutboxAsync(Dead);

        var results = await Store.AbandonAsync([dead, pending, alreadyAbandoned, outboxDead, Guid.CreateVersion7()], "对方已经不存在", Ct);

        results.Select(r => r.Outcome).ShouldBe([MessageActionOutcome.Done, MessageActionOutcome.WrongState, MessageActionOutcome.WrongState, MessageActionOutcome.NotFound, MessageActionOutcome.NotFound]);
        var row = await RowAsync(MessageBox.Inbox, dead);
        row.Status.ShouldBe(Abandoned);
        row.LastError.ShouldNotBeNull().ShouldContain("对方已经不存在");
        row.LastError.ShouldContain("原来的错误");
        (await RowAsync(MessageBox.Inbox, pending)).Status.ShouldBe(Pending);
        (await RowAsync(MessageBox.Outbox, outboxDead)).Status.ShouldBe(Dead, "发件箱的死信不能放弃，只能重试");
        await Should.ThrowAsync<ArgumentException>(async () => await Store.AbandonAsync([dead], " ", Ct));
    }

    private static DateTimeOffset Hours(double hours) => TimeProvider.System.GetUtcNow().AddHours(hours);

    private Task<Guid> InsertAsync(MessageBox box, short status, int attempts = 0) =>
        box == MessageBox.Inbox ? InsertInboxAsync("Echo.Generic" + _unique, status, attempts: attempts) : InsertOutboxAsync(status, attempts: attempts);

    private async Task<Guid> InsertOutboxAsync(short status, string? type = null, DateTimeOffset? occurred = null, string? error = null, string payload = "{}", string headers = "{}", int attempts = 0)
    {
        var id = Guid.CreateVersion7();
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO {Tables.For(Module).Outbox} ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "OccurredAt", "Status", "Attempts", "LastError")
            VALUES (@id, @type, 1, 0, @payload::jsonb, @headers::jsonb, @occurred, @status, @attempts, @error)
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("type", type ?? "Test.Outbox" + _unique);
        command.Parameters.AddWithValue("payload", payload);
        command.Parameters.AddWithValue("headers", headers);
        command.Parameters.AddWithValue("occurred", occurred ?? TimeProvider.System.GetUtcNow());
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("attempts", attempts);
        command.Parameters.AddWithValue("error", (object?)error ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(Ct);
        return id;
    }

    private async Task<Guid> InsertInboxAsync(
        string subscription, short status, string? type = null, DateTimeOffset? occurred = null, DateTimeOffset? received = null, string? key = null, string? error = null, string payload = "{}", string headers = "{}", int attempts = 0)
    {
        var id = Guid.CreateVersion7();
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO {Tables.For(Module).Inbox} ("Id", "MessageId", "Subscription", "MessageType", "SchemaVersion", "Payload", "Headers", "SourceModule", "OrderingKey", "Seq", "OccurredAt", "ReceivedAt", "Status", "Attempts", "LastError")
            VALUES (@id, @messageId, @subscription, @type, 1, @payload::jsonb, @headers::jsonb, 'Probe', @key, @seq, @occurred, @received, @status, @attempts, @error)
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("messageId", Guid.CreateVersion7());
        command.Parameters.AddWithValue("subscription", subscription);
        command.Parameters.AddWithValue("type", type ?? "Test.Inbox" + _unique);
        command.Parameters.AddWithValue("payload", payload);
        command.Parameters.AddWithValue("headers", headers);
        command.Parameters.AddWithValue("key", (object?)key ?? DBNull.Value);
        command.Parameters.AddWithValue("seq", _seq++);
        command.Parameters.AddWithValue("occurred", occurred ?? TimeProvider.System.GetUtcNow());
        command.Parameters.AddWithValue("received", received ?? TimeProvider.System.GetUtcNow());
        command.Parameters.AddWithValue("status", status);
        command.Parameters.AddWithValue("attempts", attempts);
        command.Parameters.AddWithValue("error", (object?)error ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(Ct);
        return id;
    }

    private async Task<RowState> RowAsync(MessageBox box, Guid id)
    {
        var table = box == MessageBox.Inbox ? Tables.For(Module).Inbox : Tables.For(Module).Outbox;
        await using var command = database.DataSource.CreateCommand($"""SELECT "Status", "Attempts", "NextAttemptAt", "LeaseToken", "LastError" FROM {table} WHERE "Id" = @id""");
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        (await reader.ReadAsync(Ct)).ShouldBeTrue();
        return new RowState(
            reader.GetInt16(0),
            reader.GetInt32(1),
            await reader.GetFieldValueAsync<DateTimeOffset>(2, Ct),
            await reader.IsDBNullAsync(3, Ct) ? null : reader.GetGuid(3),
            await reader.IsDBNullAsync(4, Ct) ? null : reader.GetString(4));
    }

    private sealed record RowState(short Status, int Attempts, DateTimeOffset NextAttemptAt, Guid? LeaseToken, string? LastError);
}
