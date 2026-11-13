using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NpgsqlTypes;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 探查未完成的消息：各模块的发件箱与收件箱都要查，只有待处理、处理中、死信算未完成，按消息类型和主体匹配
/// </summary>
public sealed class UnfinishedMessageProbeTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const short Pending = 0;
    private const short Processing = 1;
    private const short Done = 2;
    private const short Dead = 3;
    private const short Abandoned = 4;

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private readonly TestIdentity _user = TestIdentity.User("探查的人").InTree(_org).InOrgUnit(_org["总部"]);
    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly string _type = "Test.Attach" + Guid.CreateVersion7().ToString("N");
    private long _seq = 1;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ModuleFrameworkTables Tables => _harness.Services.GetRequiredService<ModuleFrameworkTables>();

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task<IReadOnlyList<Guid>> FindAsync(params Guid[] ids)
    {
        await using var work = _harness.Begin(_user);
        var result = await work.Sender.SendAsync(new FindUnfinishedCommand(_type, ids), Ct);
        return result.ShouldBeSuccess().Ids;
    }

    [Fact]
    public async Task OutboxMessages_NotYetDelivered_ArePickedUp_ButDeliveredOnesAreNot()
    {
        var pending = Guid.CreateVersion7();
        var processing = Guid.CreateVersion7();
        var dead = Guid.CreateVersion7();
        var done = Guid.CreateVersion7();
        await InsertOutboxAsync("BuildingBlocks", Pending, [pending]);
        await InsertOutboxAsync("BuildingBlocks", Processing, [processing]);
        await InsertOutboxAsync("BuildingBlocks", Dead, [dead]);
        await InsertOutboxAsync("BuildingBlocks", Done, [done]);

        var found = await FindAsync(pending, processing, dead, done);

        found.ShouldBe([pending, processing, dead], ignoreOrder: true);
    }

    [Fact]
    public async Task InboxMessages_OfAnyModule_AreCheckedToo_AndAbandonedOnesAreNot()
    {
        var waiting = Guid.CreateVersion7();
        var stuck = Guid.CreateVersion7();
        var abandoned = Guid.CreateVersion7();
        var handled = Guid.CreateVersion7();
        await InsertInboxAsync("Echo", Pending, waiting);
        await InsertInboxAsync("BuildingBlocks", Dead, stuck);
        await InsertInboxAsync("Echo", Abandoned, abandoned);
        await InsertInboxAsync("Echo", Done, handled);

        var found = await FindAsync(waiting, stuck, abandoned, handled);

        found.ShouldBe([waiting, stuck], ignoreOrder: true);
    }

    [Fact]
    public async Task OnlyTheAskedSubjectsAndTheAskedMessageType_AreReported()
    {
        var mentioned = Guid.CreateVersion7();
        var alongside = Guid.CreateVersion7();
        var otherType = Guid.CreateVersion7();
        await InsertOutboxAsync("BuildingBlocks", Pending, [mentioned, alongside]);
        await InsertOutboxAsync("BuildingBlocks", Pending, [otherType], _type + ".Other");

        // 一条消息提到的其他对象不会被带出来，别的消息类型也不算
        (await FindAsync(mentioned, otherType)).ShouldBe([mentioned]);
    }

    [Fact]
    public async Task NothingToAsk_IsNothingFound_AndAMessageWithoutSubjectsIsIgnored()
    {
        var none = Guid.CreateVersion7();
        await InsertOutboxAsync("BuildingBlocks", Pending, []);

        (await FindAsync()).ShouldBeEmpty();
        (await FindAsync(none)).ShouldBeEmpty();
    }

    [Fact]
    public async Task OutsideACommand_ItRefuses()
    {
        await using var scope = _harness.Services.CreateAsyncScope();
        var probe = scope.ServiceProvider.GetRequiredService<IUnfinishedMessageProbe>();

        await Should.ThrowAsync<InvalidOperationException>(async () => await probe.FindSubjectsAsync(_type, [Guid.CreateVersion7()], Ct));
    }

    private async Task InsertOutboxAsync(string module, short status, Guid[] subjects, string? type = null)
    {
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO {Tables.For(module).Outbox} ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "SubjectIds", "OccurredAt", "Status", "Attempts")
            VALUES (@id, @type, 1, 0, @payload::jsonb, @headers::jsonb, @subjects, @occurred, @status, 0)
            """);
        command.Parameters.AddWithValue("id", Guid.CreateVersion7());
        command.Parameters.AddWithValue("type", type ?? _type);
        command.Parameters.AddWithValue("payload", "{}");
        command.Parameters.AddWithValue("headers", "{}");
        command.Parameters.Add(new NpgsqlParameter("subjects", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = subjects.Length == 0 ? DBNull.Value : subjects });
        command.Parameters.AddWithValue("occurred", TimeProvider.System.GetUtcNow());
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync(Ct);
    }
    private async Task InsertInboxAsync(string module, short status, Guid subject)
    {
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO {Tables.For(module).Inbox} ("Id", "MessageId", "Subscription", "MessageType", "SchemaVersion", "Payload", "Headers", "SourceModule", "Seq", "SubjectIds", "OccurredAt", "ReceivedAt", "Status", "Attempts")
            VALUES (@id, @messageId, 'Test.Subscription', @type, 1, @payload::jsonb, @headers::jsonb, 'Probe', @seq, @subjects, @now, @now, @status, 0)
            """);
        command.Parameters.AddWithValue("id", Guid.CreateVersion7());
        command.Parameters.AddWithValue("messageId", Guid.CreateVersion7());
        command.Parameters.AddWithValue("type", _type);
        command.Parameters.AddWithValue("payload", "{}");
        command.Parameters.AddWithValue("headers", "{}");
        command.Parameters.AddWithValue("seq", _seq++);
        command.Parameters.Add(new NpgsqlParameter("subjects", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = new[] { subject } });
        command.Parameters.AddWithValue("now", TimeProvider.System.GetUtcNow());
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
