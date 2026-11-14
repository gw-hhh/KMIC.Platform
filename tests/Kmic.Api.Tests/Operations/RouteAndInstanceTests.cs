using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 订阅路由与运行中的实例：宿主启动后登记路由和心跳，列表能看到每个订阅现在由哪些构建声明，端到端
/// </summary>
public sealed class RouteAndInstanceTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations/messages";

    private static readonly string[] _view = [OperationsPermissions.MessagesView];

    private static bool Is(JsonElement element, string property, string expected) =>
        string.Equals(element.GetProperty(property).GetString(), expected, StringComparison.Ordinal);

    private async Task<JsonElement> InstancesAsync(string token)
    {
        var response = await SendAsync(HttpMethod.Get, $"{Root}/instances", token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(response);
    }

    // 宿主第一次被用到才启动，心跳在启动任务做完之后登记，所以要等一等
    private async Task<JsonElement> LiveInstanceAsync(string token) =>
        await Eventually.UntilAsync(
            async _ => (await InstancesAsync(token)).EnumerateArray().FirstOrDefault(i => i.GetProperty("isAlive").GetBoolean()),
            instance => instance.ValueKind == JsonValueKind.Object,
            "宿主登记了心跳",
            TimeSpan.FromSeconds(20),
            Ct);

    [Fact]
    public async Task TheRunningHost_RegistersItsBuildRolesAndSubscriptions()
    {
        var token = await LoginAsOperatorAsync("route-instances-viewer", _view);

        var instance = await LiveInstanceAsync(token);

        instance.GetProperty("roles").GetString()!.ShouldContain("Api");
        instance.GetProperty("build").GetString().ShouldNotBeNullOrWhiteSpace();
        instance.GetProperty("subscriptions").EnumerateArray()
            .Any(s => Is(s, "subscription", "Operations.RecordAudit") && Is(s, "module", "Operations"))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task AnInstanceThatStoppedBeating_IsListedAsNotAlive_AfterTheLiveOnes()
    {
        var token = await LoginAsOperatorAsync("route-instances-stale", _view);
        await LiveInstanceAsync(token);
        var stale = Guid.CreateVersion7();
        await Seed.SqlAsync(
            """
            INSERT INTO "Operations"."MessagingInstance" ("Id", "Host", "Roles", "Build", "Subscriptions", "StartedAt", "HeartbeatAt")
            VALUES (@id, 'old-host', 'Worker', 'old-build', '[]'::jsonb, now() - interval '3 hours', now() - interval '1 hour')
            """,
            ("id", stale));

        var instances = (await InstancesAsync(token)).EnumerateArray().ToList();

        var old = instances.Single(i => i.GetProperty("id").GetGuid() == stale);
        old.GetProperty("isAlive").GetBoolean().ShouldBeFalse();
        instances.IndexOf(old).ShouldBeGreaterThan(instances.FindLastIndex(i => i.GetProperty("isAlive").GetBoolean()));
    }

    [Fact]
    public async Task TheRoutes_ShowTheirStatus_AndWhichLiveBuildsDeclareThem()
    {
        var token = await LoginAsOperatorAsync("route-list-viewer", _view);
        var live = await LiveInstanceAsync(token);
        var marker = Guid.CreateVersion7().ToString("N");
        await Seed.SqlAsync(
            """
            INSERT INTO "Operations"."MessageRoute" ("MessageType", "Subscription", "Module", "Status", "RegisteredBuild", "RetiredReason")
            VALUES (@type, @retiring, 'Operations', 1, 'build-1', '不再需要'), (@type, @finalized, 'Operations', 2, 'build-1', '已经终结')
            """,
            ("type", "Test." + marker), ("retiring", "Test.Retiring." + marker), ("finalized", "Test.Finalized." + marker));

        var all = await JsonAsync(await SendAsync(HttpMethod.Get, $"{Root}/routes", token));
        var retiringOnly = await JsonAsync(await SendAsync(HttpMethod.Get, $"{Root}/routes?status=Retiring", token));

        var audit = all.EnumerateArray().Single(r => Is(r, "subscription", "Operations.RecordAudit"));
        audit.GetProperty("status").GetString().ShouldBe("Active");
        audit.GetProperty("declaredByBuilds").EnumerateArray().Any(b => Is(b, "build", live.GetProperty("build").GetString()!)).ShouldBeTrue();
        var retiring = all.EnumerateArray().Single(r => Is(r, "subscription", "Test.Retiring." + marker));
        retiring.GetProperty("status").GetString().ShouldBe("Retiring");
        retiring.GetProperty("retiredReason").GetString().ShouldBe("不再需要");
        retiring.GetProperty("declaredByBuilds").GetArrayLength().ShouldBe(0, "没有运行中的实例还在声明它");
        retiringOnly.EnumerateArray().ShouldAllBe(r => Is(r, "status", "Retiring"));
        retiringOnly.EnumerateArray().Any(r => Is(r, "subscription", "Test.Retiring." + marker)).ShouldBeTrue();
    }

    [Fact]
    public async Task RoutesAndInstances_NeedThePermission_AndAKnownStatus()
    {
        var plain = await LoginAsOperatorAsync("route-plain", []);
        var viewer = await LoginAsOperatorAsync("route-status-viewer", _view);

        (await SendAsync(HttpMethod.Get, $"{Root}/routes", plain)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await SendAsync(HttpMethod.Get, $"{Root}/instances", plain)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await SendAsync(HttpMethod.Get, $"{Root}/routes?status=Nonsense", viewer)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
