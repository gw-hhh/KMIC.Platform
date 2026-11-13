using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Deliveries.Purge;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 投递管理：列表与详情、重试、放弃，以及过期投递和含密通知残留正文的清理。关闭了捕获，投递停在待发送，状态由测试直接改
/// </summary>
public sealed class DeliveryManagementTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(
        database,
        template,
        new Dictionary<string, string>(StringComparer.Ordinal) { ["Notifications:Capture:Enabled"] = "false" }),
    IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/notifications/deliveries";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private static int _users;

    private HttpClient ClientWith(string name, params string[] permissions)
    {
        var identity = TestIdentity.User(name).InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(permissions).Scope(ScopeLevel.All));
        Factory.Loader.Register(identity);
        return ClientFor(identity);
    }

    private HttpClient Admin() => ClientWith("投递管理员", NotificationsPermissions.DeliveryView, NotificationsPermissions.DeliveryManage);

    private async Task SendSubmittedAsync(string dedup, Guid user, string number)
    {
        var message = SendNotification.For(new SpecimenSubmittedNotice(number, "样品", "/sample/specimens/7"), [Recipient.OfUser(user)], dedup);
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private async Task SendSecretAsync(string dedup, Guid user)
    {
        var message = SendSecretNotification.For(new SpecimenCode("111111"), [Recipient.OfUser(user)], dedup);
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendSecretNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private async Task RunAsync<T>(ICommand<T> command)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.BackgroundJob);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(command, Ct);
        result.ShouldBeSuccess();
    }

    // 手机号和邮箱在用户表里唯一，每个用户给不同的
    private static async Task<Guid> UserAsync(IdentitySeed seed)
    {
        var number = Interlocked.Increment(ref _users);
        return await seed.UserAsync($"dm-{Guid.CreateVersion7():N}", null, displayName: "收通知的人", mobile: $"+861390000{number:D4}", email: $"dm-{number}@example.com");
    }

    private static async Task<Guid> DeliveryIdAsync(IdentitySeed seed, string dedup) =>
        await seed.ScalarAsync<Guid>($"""SELECT "Id" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' LIMIT 1""");

    private static async Task<string?> RowAsync(IdentitySeed seed, Guid id) =>
        await seed.ScalarAsync<string>($"""SELECT "Status" || '|' || "Attempts" || '|' || coalesce("LastError", '-') FROM "Notifications"."Delivery" WHERE "Id" = '{id}'""");

    private static async Task FailAsync(IdentitySeed seed, Guid id, string extra = "") =>
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "Status" = 'Failed', "Attempts" = 5, "LastError" = 'boom'{extra} WHERE "Id" = '{id}'""");

    [Fact]
    public async Task List_ShowsTheDeliveryWithoutItsBody_AndFiltersByStatus()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        await SendSubmittedAsync($"list-{Guid.CreateVersion7():N}", user, "SP-1");
        using var admin = Admin();

        var list = await JsonAsync(await admin.GetAsync($"{Root}?recipientUserId={user}", Ct));

        var item = list.GetProperty("items").EnumerateArray().Single();
        item.GetProperty("status").GetString().ShouldBe("Pending");
        item.GetProperty("channel").GetString().ShouldBe("Email");
        item.GetProperty("addressMasked").GetString().ShouldBe("d***@example.com");
        item.GetProperty("attempts").GetInt32().ShouldBe(0);
        item.TryGetProperty("body", out _).ShouldBeFalse();
        var failed = await JsonAsync(await admin.GetAsync($"{Root}?recipientUserId={user}&status=Failed", Ct));
        failed.GetProperty("items").GetArrayLength().ShouldBe(0);
        (await admin.GetAsync($"{Root}?status=Nope", Ct)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_ShowsTheRenderedSubjectAndBody()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        var dedup = $"get-{Guid.CreateVersion7():N}";
        await SendSubmittedAsync(dedup, user, "SP-2");
        using var admin = Admin();

        var detail = await JsonAsync(await admin.GetAsync($"{Root}/{await DeliveryIdAsync(seed, dedup)}", Ct));

        detail.GetProperty("subject").GetString().ShouldNotBeNull().ShouldContain("SP-2");
        detail.GetProperty("body").GetString().ShouldNotBeNullOrEmpty();
        detail.GetProperty("templateKey").GetString().ShouldBe("Sample.SpecimenSubmitted");
        (await admin.GetAsync($"{Root}/{Guid.CreateVersion7()}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Retry_PutsAFailedDeliveryBackInTheQueue_ButNotOneThatIsNotFailed()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        var dedup = $"retry-{Guid.CreateVersion7():N}";
        await SendSubmittedAsync(dedup, user, "SP-3");
        var id = await DeliveryIdAsync(seed, dedup);
        using var admin = Admin();
        (await PostActionAsync(admin, $"{Root}/{id}/retry", new { })).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        await FailAsync(seed, id);

        var retried = await PostActionAsync(admin, $"{Root}/{id}/retry", new { });

        retried.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await RowAsync(seed, id)).ShouldBe("Pending|0|-");
    }

    [Fact]
    public async Task Retry_RefusesASecretNotificationPastItsLifetime_ButAllowsOneStillInIt()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        var expired = $"secret-old-{Guid.CreateVersion7():N}";
        var fresh = $"secret-new-{Guid.CreateVersion7():N}";
        await SendSecretAsync(expired, user);
        await SendSecretAsync(fresh, user);
        var expiredId = await DeliveryIdAsync(seed, expired);
        var freshId = await DeliveryIdAsync(seed, fresh);
        await FailAsync(seed, expiredId, ", \"CreatedAt\" = now() - interval '2 hours'");
        await FailAsync(seed, freshId);
        using var admin = Admin();

        (await PostActionAsync(admin, $"{Root}/{expiredId}/retry", new { })).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await PostActionAsync(admin, $"{Root}/{freshId}/retry", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await RowAsync(seed, expiredId)).ShouldStartWith("Failed|");
        (await RowAsync(seed, freshId)).ShouldStartWith("Pending|");
    }

    [Fact]
    public async Task Abandon_MarksAFailedDelivery_NeedsAReason_AndStopsRetries()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        var dedup = $"abandon-{Guid.CreateVersion7():N}";
        await SendSubmittedAsync(dedup, user, "SP-4");
        var id = await DeliveryIdAsync(seed, dedup);
        await FailAsync(seed, id);
        using var admin = Admin();

        (await PostActionAsync(admin, $"{Root}/{id}/abandon", new { reason = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await PostActionAsync(admin, $"{Root}/{id}/abandon", new { reason = "收件人确认不需要" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var list = await JsonAsync(await admin.GetAsync($"{Root}?recipientUserId={user}", Ct));
        list.GetProperty("items").EnumerateArray().Single().GetProperty("abandoned").GetBoolean().ShouldBeTrue();
        (await PostActionAsync(admin, $"{Root}/{id}/retry", new { })).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await PostActionAsync(admin, $"{Root}/{id}/abandon", new { reason = "再放弃一次" })).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Permissions_ViewingDoesNotAllowHandlingFailures_AndNobodyReadsWithoutLoggingIn()
    {
        using var viewer = ClientWith("投递查看员", NotificationsPermissions.DeliveryView);
        using var anonymous = ClientFor();

        (await viewer.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PostActionAsync(viewer, $"{Root}/{Guid.CreateVersion7()}/retry", new { })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await anonymous.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PurgeDeliveries_DeletesOldFinishedOnes_AndKeepsTheRest()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        var tag = Guid.CreateVersion7().ToString("N");
        string[] keys = [$"purge-sent-old-{tag}", $"purge-failed-open-{tag}", $"purge-failed-abandoned-{tag}", $"purge-sent-recent-{tag}"];
        foreach (var dedup in keys)
        {
            await SendSubmittedAsync(dedup, user, "SP-5");
        }

        var old = "\"CreatedAt\" = now() - interval '200 days'";
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "Status" = 'Sent', {old} WHERE "DedupKey" = '{keys[0]}'""");
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "Status" = 'Failed', {old} WHERE "DedupKey" = '{keys[1]}'""");
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "Status" = 'Failed', "AbandonedAt" = now() - interval '199 days', {old} WHERE "DedupKey" = '{keys[2]}'""");
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "Status" = 'Sent' WHERE "DedupKey" = '{keys[3]}'""");
        await seed.SqlAsync($"""UPDATE "Notifications"."Notification" SET {old} WHERE "DedupKey" IN ('{keys[0]}', '{keys[1]}', '{keys[2]}')""");

        await RunAsync(new PurgeDeliveriesCommand(TimeProvider.System.GetUtcNow()));

        var left = await seed.ScalarAsync<string>($"""SELECT string_agg("DedupKey", ',' ORDER BY "DedupKey") FROM "Notifications"."Delivery" WHERE "DedupKey" LIKE 'purge-%-{tag}'""");
        left.ShouldBe($"{keys[1]},{keys[3]}");
        var notifications = await seed.ScalarAsync<string>($"""SELECT string_agg("DedupKey", ',' ORDER BY "DedupKey") FROM "Notifications"."Notification" WHERE "DedupKey" LIKE 'purge-%-{tag}'""");
        notifications.ShouldBe($"{keys[1]},{keys[3]}", "发送请求记录在它的投递都删完之后才删");
    }

    [Fact]
    public async Task ClearSecretBodies_EmptiesLeftoverSecretContent_AndLeavesOrdinaryDeliveriesAlone()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);
        var secret = $"clear-secret-{Guid.CreateVersion7():N}";
        var ordinary = $"clear-ordinary-{Guid.CreateVersion7():N}";
        await SendSecretAsync(secret, user);
        await SendSubmittedAsync(ordinary, user, "SP-6");
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "Status" = 'Sent' WHERE "DedupKey" IN ('{secret}', '{ordinary}')""");

        await RunAsync(new ClearSecretBodiesCommand(TimeProvider.System.GetUtcNow()));

        (await seed.ScalarAsync<bool>($"""SELECT "Body" IS NULL AND "SmsParameters" IS NULL FROM "Notifications"."Delivery" WHERE "DedupKey" = '{secret}'""")).ShouldBeTrue();
        (await seed.ScalarAsync<bool>($"""SELECT "Body" IS NOT NULL FROM "Notifications"."Delivery" WHERE "DedupKey" = '{ordinary}'""")).ShouldBeTrue();
    }

    [Fact]
    public async Task Captured_IsNotFound_WhenCapturingIsOff()
    {
        using var client = ClientWith("捕获查看员", NotificationsPermissions.CapturedView);

        (await client.GetAsync("/api/v1/notifications/captured", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await client.GetAsync($"/api/v1/notifications/captured/{Guid.CreateVersion7()}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
