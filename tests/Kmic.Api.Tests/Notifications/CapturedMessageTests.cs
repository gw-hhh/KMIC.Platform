using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Captured.Purge;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 非生产环境捕获的内容：查看列表和详情、权限、过期清理。捕获默认开启
/// </summary>
public sealed class CapturedMessageTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/notifications/captured";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private HttpClient ClientWith(string name, params string[] permissions)
    {
        var identity = TestIdentity.User(name).InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(permissions).Scope(ScopeLevel.All));
        Factory.Loader.Register(identity);
        return ClientFor(identity);
    }

    private async Task SendSubmittedAsync(string dedup, Guid user, string number)
    {
        var message = SendNotification.For(new SpecimenSubmittedNotice(number, "样品", "/sample/specimens/7"), [Recipient.OfUser(user)], dedup);
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private static async Task<Guid> UserAsync(IdentitySeed seed, string email) =>
        await seed.UserAsync($"cap-{Guid.CreateVersion7():N}", null, displayName: "收通知的人", email: email);

    [Fact]
    public async Task ACapturedEmail_ShowsInTheList_AndItsBodyOnlyInTheDetail()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var email = $"cap-{Guid.CreateVersion7():N}@example.com";
        var user = await UserAsync(seed, email);
        await SendSubmittedAsync($"captured-{Guid.CreateVersion7():N}", user, "SP-CAP");
        using var viewer = ClientWith("捕获查看员", NotificationsPermissions.CapturedView);

        var list = await JsonAsync(await viewer.GetAsync($"{Root}?channel=Email&pageSize=200", Ct));

        var item = list.GetProperty("items").EnumerateArray().Single(i => string.Equals(i.GetProperty("address").GetString(), email, StringComparison.Ordinal));
        item.GetProperty("subject").GetString().ShouldNotBeNull().ShouldContain("SP-CAP");
        item.TryGetProperty("body", out _).ShouldBeFalse();
        var detail = await JsonAsync(await viewer.GetAsync($"{Root}/{item.GetProperty("id").GetGuid()}", Ct));
        detail.GetProperty("body").GetString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Permissions_ViewingCapturedContentNeedsItsOwnPermission()
    {
        using var other = ClientWith("没有捕获权限的人", NotificationsPermissions.DeliveryView);
        using var anonymous = ClientFor();

        (await other.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await anonymous.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PurgeCaptured_DeletesOldCapturedContent_AndKeepsRecentOnes()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, $"cap-{Guid.CreateVersion7():N}@example.com");
        var tag = Guid.CreateVersion7().ToString("N");
        await SendSubmittedAsync($"cap-old-{tag}", user, "SP-OLD");
        await SendSubmittedAsync($"cap-new-{tag}", user, "SP-NEW");
        await seed.SqlAsync($"""UPDATE "Notifications"."CapturedMessage" SET "CapturedAt" = now() - interval '40 days' WHERE "DeliveryId" IN (SELECT "Id" FROM "Notifications"."Delivery" WHERE "DedupKey" = 'cap-old-{tag}')""");

        await using (var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.BackgroundJob))
        {
            (await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new PurgeCapturedCommand(TimeProvider.System.GetUtcNow()), Ct)).ShouldBeSuccess();
        }

        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."CapturedMessage" c JOIN "Notifications"."Delivery" d ON d."Id" = c."DeliveryId" WHERE d."DedupKey" = 'cap-old-{tag}'""")).ShouldBe(0);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."CapturedMessage" c JOIN "Notifications"."Delivery" d ON d."Id" = c."DeliveryId" WHERE d."DedupKey" = 'cap-new-{tag}'""")).ShouldBe(1);
    }
}
