using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 补投：只补已经投递过的、序号落在区间里的消息；订阅收到过的不重复写入；订阅已终结、来源或订阅不对时拒绝，端到端
/// </summary>
/// <remarks>
/// 发件箱的行直接写进 Sample 的发件箱，订阅方也借用 Sample 的收件箱
/// </remarks>
public sealed class RedeliverTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Url = "/api/v1/operations/messages/redeliver";
    private const short Pending = 0;
    private const short Done = 2;

    private static readonly string[] _view = [OperationsPermissions.MessagesView];
    private static readonly string[] _manage = [OperationsPermissions.MessagesView, OperationsPermissions.MessagesManage];

    private async Task<(string Type, string Subscription)> RouteAsync(short status = 0)
    {
        var unique = Guid.CreateVersion7().ToString("N");
        var type = "Test.Redeliver." + unique;
        var subscription = "Test.Redeliver.Subscription." + unique;
        await Seed.SqlAsync(
            """
            INSERT INTO "Operations"."MessageRoute" ("MessageType", "Subscription", "Module", "Status", "RegisteredBuild")
            VALUES (@type, @subscription, 'Sample', @status, 'build-1')
            """,
            ("type", type), ("subscription", subscription), ("status", status));
        return (type, subscription);
    }

    private async Task<long> OutboxAsync(string type, short status)
    {
        return await Seed.ScalarAsync<long>(
            """
            INSERT INTO "Sample"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "OccurredAt", "Status", "Attempts")
            VALUES (@id, @type, 1, 0, '{"x":1}'::jsonb, '{}'::jsonb, now(), @status, 1)
            RETURNING "Seq"
            """,
            ("id", Guid.CreateVersion7()), ("type", type), ("status", status));
    }

    private Task<HttpResponseMessage> RedeliverAsync(string token, string type, string subscription, long from, long to, string source = "Sample") =>
        SendAsync(HttpMethod.Post, Url, token, new { sourceModule = source, messageType = type, subscription, fromSeq = from, toSeq = to });

    private Task<long> InboxRowsAsync(string subscription) =>
        Seed.CountAsync($"""SELECT count(*) FROM "Sample"."InboxMessage" WHERE "Subscription" = '{subscription}'""");

    [Fact]
    public async Task OnlyDeliveredMessagesInTheRange_AreWritten_AndASecondRedeliveryWritesNothing()
    {
        var token = await LoginAsOperatorAsync("redeliver-basic", _manage);
        var (type, subscription) = await RouteAsync();
        var first = await OutboxAsync(type, Done);
        var second = await OutboxAsync(type, Done);
        var notDelivered = await OutboxAsync(type, Pending);
        var outside = await OutboxAsync(type, Done);

        var response = await RedeliverAsync(token, type, subscription, first, notDelivered);
        var again = await RedeliverAsync(token, type, subscription, first, notDelivered);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await JsonAsync(response);
        body.GetProperty("candidates").GetInt64().ShouldBe(2, "区间里还没投递的不补");
        body.GetProperty("written").GetInt64().ShouldBe(2);
        (await JsonAsync(again)).GetProperty("written").GetInt64().ShouldBe(0, "订阅已经收到过的不再写");
        (await InboxRowsAsync(subscription)).ShouldBe(2);
        (await Seed.CountAsync($"""SELECT count(*) FROM "Sample"."InboxMessage" WHERE "Subscription" = '{subscription}' AND "Status" = {Pending} AND "SourceModule" = 'Sample' AND "Seq" IN ({first}, {second})""")).ShouldBe(2, "序号沿用来源的序号，等处理程序领取");
        outside.ShouldBeGreaterThan(notDelivered);
    }

    [Fact]
    public async Task AFinalizedRoute_AnUnknownRoute_AndAnUnknownSource_AreRefused()
    {
        var token = await LoginAsOperatorAsync("redeliver-refused", _manage);
        var (type, subscription) = await RouteAsync(status: 2);
        var seq = await OutboxAsync(type, Done);

        var finalized = await RedeliverAsync(token, type, subscription, seq, seq);
        var unknownRoute = await RedeliverAsync(token, "Test.Nothing", "Test.Nothing", 1, 10);
        var unknownSource = await RedeliverAsync(token, type, subscription, seq, seq, source: "NoSuchModule");
        var backwards = await RedeliverAsync(token, type, subscription, 10, 5);

        finalized.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        unknownRoute.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        unknownSource.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        backwards.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await InboxRowsAsync(subscription)).ShouldBe(0);
    }

    [Fact]
    public async Task AnOversizedRange_IsRefusedWithoutWritingAnything()
    {
        var token = await LoginAsOperatorAsync("redeliver-too-many", _manage);
        var (type, subscription) = await RouteAsync();
        await Seed.SqlAsync(
            """
            INSERT INTO "Sample"."OutboxMessage" ("Id", "MessageType", "SchemaVersion", "Kind", "Payload", "Headers", "OccurredAt", "Status", "Attempts")
            SELECT gen_random_uuid(), @type, 1, 0, '{}'::jsonb, '{}'::jsonb, now(), 2, 1 FROM generate_series(1, 5001)
            """,
            ("type", type));
        var low = await Seed.ScalarAsync<long>("""SELECT min("Seq") FROM "Sample"."OutboxMessage" WHERE "MessageType" = @type""", ("type", type));

        var response = await RedeliverAsync(token, type, subscription, low, low + 100_000);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Operations.RedeliverTooMany");
        (await InboxRowsAsync(subscription)).ShouldBe(0);
    }

    [Fact]
    public async Task Redelivering_NeedsThePermissionToManageMessages()
    {
        var viewer = await LoginAsOperatorAsync("redeliver-viewer", _view);

        (await RedeliverAsync(viewer, "Test.Any", "Test.Any", 1, 2)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
