using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 终结与重新激活订阅：能不能终结的各种拦截，放弃剩余消息，终结之后重新激活，端到端
/// </summary>
/// <remarks>
/// 路由和收件箱的行直接写进库。订阅方借用 Sample 的收件箱
/// </remarks>
public sealed class RouteFinalizeTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations/messages/routes";
    private const short Pending = 0;
    private const short Processing = 1;
    private const short Dead = 3;

    private static readonly string[] _view = [OperationsPermissions.MessagesView];
    private static readonly string[] _manage = [OperationsPermissions.MessagesView, OperationsPermissions.MessagesManage];

    private long _seq = 5_000_000;

    private async Task<(string Type, string Subscription)> RouteAsync(short status)
    {
        var unique = Guid.CreateVersion7().ToString("N");
        var type = "Test.Message." + unique;
        var subscription = "Test.Subscription." + unique;
        await Seed.SqlAsync(
            """
            INSERT INTO "Operations"."MessageRoute" ("MessageType", "Subscription", "Module", "Status", "RegisteredBuild", "RetiredReason")
            VALUES (@type, @subscription, 'Sample', @status, 'build-1', '不再需要')
            """,
            ("type", type), ("subscription", subscription), ("status", status));
        return (type, subscription);
    }

    private async Task<Guid> InboxAsync(string type, string subscription, short status, bool leaseValid = false)
    {
        var id = Guid.CreateVersion7();
        await Seed.SqlAsync(
            """
            INSERT INTO "Sample"."InboxMessage" ("Id", "MessageId", "Subscription", "MessageType", "SchemaVersion", "Payload", "Headers", "SourceModule", "Seq", "OccurredAt", "ReceivedAt", "Status", "Attempts", "LastError", "LeaseUntil")
            VALUES (@id, @message, @subscription, @type, 1, '{}'::jsonb, '{}'::jsonb, 'Probe', @seq, now(), now(), @status, 1, '原来的错误',
                    CASE WHEN @status = 1 THEN now() + CASE WHEN @leaseValid THEN interval '5 minutes' ELSE interval '-5 minutes' END END)
            """,
            ("id", id), ("message", Guid.CreateVersion7()), ("subscription", subscription), ("type", type), ("seq", _seq++), ("status", status), ("leaseValid", leaseValid));
        return id;
    }

    private Task<HttpResponseMessage> FinalizeAsync(string token, string type, string subscription, bool abandon = false, string reason = "新版本已经全部上线") =>
        SendAsync(HttpMethod.Post, $"{Root}/finalize", token, new { messageType = type, subscription, reason, abandonUnfinished = abandon });

    private Task<HttpResponseMessage> ReactivateAsync(string token, string type, string subscription) =>
        SendAsync(HttpMethod.Post, $"{Root}/reactivate", token, new { messageType = type, subscription });

    private static async Task<string> CodeOfAsync(HttpResponseMessage response) => (await JsonAsync(response)).GetProperty("code").GetString()!;

    private Task<short> RouteStatusAsync(string subscription) =>
        Seed.ScalarAsync<short>("""SELECT "Status" FROM "Operations"."MessageRoute" WHERE "Subscription" = @subscription""", ("subscription", subscription));

    [Fact]
    public async Task OnlyARetiringRoute_CanBeFinalized_AndTheRouteMustExist()
    {
        var token = await LoginAsOperatorAsync("route-fin-basic", _manage);
        var (type, subscription) = await RouteAsync(status: 0);

        var active = await FinalizeAsync(token, type, subscription);
        var unknown = await FinalizeAsync(token, "Test.Nothing", "Test.Nothing");

        active.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(active)).ShouldBe("Operations.RouteNotRetiring");
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await RouteStatusAsync(subscription)).ShouldBe((short)0);
    }

    [Fact]
    public async Task ARouteThatALiveInstanceStillDeclares_CannotBeFinalized()
    {
        var token = await LoginAsOperatorAsync("route-fin-declared", _manage);
        var viewer = await LoginAsOperatorAsync("route-fin-declared-view", _view);

        // 等宿主登记了心跳，再把它声明的订阅改成待终结
        await Eventually.UntilAsync(
            async _ => await Seed.CountAsync("""SELECT count(*) FROM "Operations"."MessagingInstance" WHERE "Subscriptions" @> '[{"subscription":"Operations.RecordAudit"}]'::jsonb"""),
            count => count > 0,
            "宿主登记了心跳",
            TimeSpan.FromSeconds(20),
            Ct);
        await Seed.SqlAsync("""UPDATE "Operations"."MessageRoute" SET "Status" = 1 WHERE "Subscription" = 'Operations.RecordAudit'""");
        var type = await Seed.ScalarAsync<string>("""SELECT "MessageType" FROM "Operations"."MessageRoute" WHERE "Subscription" = 'Operations.RecordAudit'""");

        var blocked = await FinalizeAsync(token, type!, "Operations.RecordAudit");
        var viewOnly = await FinalizeAsync(viewer, type!, "Operations.RecordAudit");

        blocked.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(blocked)).ShouldBe("Operations.RouteStillDeclared");
        viewOnly.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await RouteStatusAsync("Operations.RecordAudit")).ShouldBe((short)1);
    }

    [Fact]
    public async Task AMessageBeingProcessed_BlocksIt_UnfinishedOnesNeedAChoice_AndAbandoningThemFinalizesInOneStep()
    {
        var token = await LoginAsOperatorAsync("route-fin-unfinished", _manage);
        var (type, subscription) = await RouteAsync(status: 1);
        var processing = await InboxAsync(type, subscription, Processing, leaseValid: true);
        var pending = await InboxAsync(type, subscription, Pending);
        var dead = await InboxAsync(type, subscription, Dead);

        var inFlight = await FinalizeAsync(token, type, subscription, abandon: true);
        await Seed.SqlAsync("""UPDATE "Sample"."InboxMessage" SET "LeaseUntil" = now() - interval '1 minute' WHERE "Id" = @id""", ("id", processing));
        var needsChoice = await FinalizeAsync(token, type, subscription);
        var finalized = await FinalizeAsync(token, type, subscription, abandon: true);

        inFlight.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(inFlight)).ShouldBe("Operations.RouteInFlight");
        needsChoice.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(needsChoice)).GetProperty("title").GetString()!.ShouldContain("3 条");
        finalized.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(finalized)).GetProperty("abandoned").GetInt64().ShouldBe(3);
        (await RouteStatusAsync(subscription)).ShouldBe((short)2);
        foreach (var id in new[] { processing, pending, dead })
        {
            (await Seed.ScalarAsync<short>("""SELECT "Status" FROM "Sample"."InboxMessage" WHERE "Id" = @id""", ("id", id))).ShouldBe((short)4, "没处理完的都放弃了");
        }

        (await Seed.ScalarAsync<string>("""SELECT "LastError" FROM "Sample"."InboxMessage" WHERE "Id" = @id""", ("id", dead))).ShouldBe("终结订阅时放弃：新版本已经全部上线；此前的错误：原来的错误");
        (await Seed.ScalarAsync<bool>("""SELECT "FinalizedAt" IS NOT NULL AND "FinalizedBy" IS NOT NULL FROM "Operations"."MessageRoute" WHERE "Subscription" = @subscription""", ("subscription", subscription))).ShouldBeTrue();
    }

    [Fact]
    public async Task ARouteWithNothingUnfinished_IsFinalizedWithoutChoosingToAbandon_AndCanBeReactivatedOnce()
    {
        var token = await LoginAsOperatorAsync("route-fin-reactivate", _manage);
        var (type, subscription) = await RouteAsync(status: 1);
        await InboxAsync(type, subscription, status: 2);

        var finalized = await FinalizeAsync(token, type, subscription);
        var again = await FinalizeAsync(token, type, subscription);
        var reactivated = await ReactivateAsync(token, type, subscription);
        var reactivatedAgain = await ReactivateAsync(token, type, subscription);
        var unknown = await ReactivateAsync(token, "Test.Nothing", "Test.Nothing");

        finalized.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(finalized)).GetProperty("abandoned").GetInt64().ShouldBe(0);
        again.StatusCode.ShouldBe(HttpStatusCode.Conflict, "已经终结的不能再终结");
        reactivated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await RouteStatusAsync(subscription)).ShouldBe((short)0);
        (await Seed.ScalarAsync<bool>("""SELECT "FinalizedAt" IS NULL AND "FinalizedBy" IS NULL AND "RetiredReason" IS NULL FROM "Operations"."MessageRoute" WHERE "Subscription" = @subscription""", ("subscription", subscription))).ShouldBeTrue();
        reactivatedAgain.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await CodeOfAsync(reactivatedAgain)).ShouldBe("Operations.RouteNotFinalized");
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TheReason_IsRequired()
    {
        var token = await LoginAsOperatorAsync("route-fin-reason", _manage);
        var (type, subscription) = await RouteAsync(status: 1);

        var response = await FinalizeAsync(token, type, subscription, reason: " ");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await RouteStatusAsync(subscription)).ShouldBe((short)1);
    }
}
