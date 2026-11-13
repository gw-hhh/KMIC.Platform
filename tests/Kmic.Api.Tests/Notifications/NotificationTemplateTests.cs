using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Notifications.Application;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Application.Templates.Sync;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 通知模板：启动时同步代码里的声明，发布版本、恢复默认、预览，以及发布之后新的通知用哪一版内容
/// </summary>
public sealed class NotificationTemplateTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/notifications/templates";
    private const string SubmittedKey = "Sample.SpecimenSubmitted";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private TestIdentity Register(string name, params string[] permissions)
    {
        var identity = TestIdentity.User(name).InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(permissions).Scope(ScopeLevel.All));
        Factory.Loader.Register(identity);
        return identity;
    }

    private HttpClient Admin() => ClientFor(Register("模板管理员", NotificationsPermissions.TemplateView, NotificationsPermissions.TemplateManage));

    private HttpClient Viewer() => ClientFor(Register("模板查看员", NotificationsPermissions.TemplateView));

    private static async Task<HttpResponseMessage> PublishAsync(HttpClient client, string channel, string subject, string body, long? expectedVersion = null, string key = SubmittedKey) =>
        await PostActionAsync(client, $"{Root}/{key}/publish", new { channel, subject, body, remark = "测试发布", expectedVersion });

    private static async Task<long> VersionOfAsync(HttpClient client, string key = SubmittedKey) =>
        (await JsonAsync(await client.GetAsync($"{Root}/{key}", Ct))).GetProperty("version").GetInt64();

    // 同一个测试类共用一个数据库，别的测试发布过的版本还在，所以版本号取最新的，不写死
    private static async Task<int> LatestVersionAsync(HttpClient client) =>
        (await JsonAsync(await client.GetAsync($"{Root}/{SubmittedKey}", Ct))).GetProperty("versions").EnumerateArray().First().GetProperty("version").GetInt32();

    private async Task<TemplateSyncResult> SyncAsync()
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new SyncTemplateCatalogCommand(), Ct);
        result.ShouldBeSuccess();
        return result.Value;
    }

    private async Task SendSubmittedAsync(string dedup, Guid user, string number)
    {
        var message = SendNotification.For(new SpecimenSubmittedNotice(number, "样品", "/sample/specimens/7"), [Recipient.OfUser(user)], dedup);
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private static async Task<Guid> UserAsync(IdentitySeed seed) =>
        await seed.UserAsync($"tpl-{Guid.CreateVersion7():N}", null, displayName: "收通知的人", email: "tpl@example.com");

    private static async Task<string?> DeliveryOfAsync(IdentitySeed seed, string dedup) =>
        await seed.ScalarAsync<string>($"""SELECT "Subject" || '|' || "Body" || '|' || "TemplateVersion" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""");

    [Fact]
    public async Task StartingTheHost_RegistersEveryDeclaredTemplate()
    {
        using var admin = Admin();

        var list = await JsonAsync(await admin.GetAsync(Root, Ct));

        var items = list.GetProperty("items").EnumerateArray().ToDictionary(i => i.GetProperty("key").GetString()!, i => i, StringComparer.Ordinal);
        items.Keys.ShouldBe(Factory.Services.GetRequiredService<NotificationTemplateCatalog>().All.Select(t => t.Key), ignoreOrder: true);
        var submitted = items[SubmittedKey];
        submitted.GetProperty("status").GetString().ShouldBe("Active");
        submitted.GetProperty("channels").EnumerateArray().Select(c => c.GetString()).ShouldBe(["Email", "InApp"]);
        submitted.GetProperty("carriesSecret").GetBoolean().ShouldBeFalse();
        items["Sample.SpecimenCode"].GetProperty("carriesSecret").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Sync_UpdatesAChangedDeclaration_RetiresATemplateThatIsGone_AndRestoresOneThatCameBack()
    {
        using var admin = Admin();
        await using var seed = new IdentitySeed(ConnectionString);
        var definition = Factory.Services.GetRequiredService<NotificationTemplateCatalog>().Find(SubmittedKey)!;
        await seed.SqlAsync("""UPDATE "Notifications"."NotificationTemplate" SET "DisplayName" = '旧名称', "CodeHash" = 'old' WHERE "Key" = 'Sample.SpecimenSubmitted'""");
        await seed.SqlAsync(
            """
            INSERT INTO "Notifications"."NotificationTemplate" ("Id", "Key", "DisplayName", "OwnerModule", "Channels", "SenderProfile", "CarriesSecret", "Status", "CodeHash", "ActiveVersions", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, 'Sample.Gone', '已删除的模板', 'Sample', 'Email', '', false, 'Active', 'x', '{}'::jsonb, now(), @by, 0)
            """,
            ("id", Guid.CreateVersion7()),
            ("by", Guid.CreateVersion7()));

        var first = await SyncAsync();

        first.ShouldBe(new TemplateSyncResult(0, 1, 1));
        (await seed.ScalarAsync<string>("""SELECT "DisplayName" FROM "Notifications"."NotificationTemplate" WHERE "Key" = 'Sample.SpecimenSubmitted'""")).ShouldBe(definition.DisplayName);
        (await seed.ScalarAsync<string>("""SELECT "Status" FROM "Notifications"."NotificationTemplate" WHERE "Key" = 'Sample.Gone'""")).ShouldBe("Retired");

        // 声明没变，再同步什么也不做；退役的模板重新出现就恢复启用
        (await SyncAsync()).ShouldBe(new TemplateSyncResult(0, 0, 0));
        await seed.SqlAsync("""UPDATE "Notifications"."NotificationTemplate" SET "Status" = 'Retired' WHERE "Key" = 'Sample.SpecimenSubmitted'""");
        (await SyncAsync()).ShouldBe(new TemplateSyncResult(0, 1, 0));
        (await seed.ScalarAsync<string>("""SELECT "Status" FROM "Notifications"."NotificationTemplate" WHERE "Key" = 'Sample.SpecimenSubmitted'""")).ShouldBe("Active");
    }

    [Fact]
    public async Task Publishing_ChangesWhatNewNotificationsSay_AndRevertingGoesBackToTheDefault()
    {
        using var admin = Admin();
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed);

        var published = await PublishAsync(admin, "Email", "新主题 {{ Number }}", "<p>{{ Title }} 已提交</p>");
        published.StatusCode.ShouldBe(HttpStatusCode.OK);
        var latest = await LatestVersionAsync(admin);
        await SendSubmittedAsync("after-publish", user, "SP-7");

        (await DeliveryOfAsync(seed, "after-publish")).ShouldBe($"新主题 SP-7|<p>样品 已提交</p>|{latest}");
        (await seed.ScalarAsync<string>("""SELECT "TemplateVersions"->>'Email' || ',' || ("TemplateVersions"->>'InApp') FROM "Notifications"."Notification" WHERE "DedupKey" = 'after-publish'""")).ShouldBe($"{latest},0");
        var title = await seed.ScalarAsync<string>($"""SELECT "Title" FROM "Notifications"."SiteMessage" WHERE "RecipientUserId" = '{user}'""");
        title.ShouldNotBeNull().ShouldNotContain("新主题", customMessage: "没发布过版本的渠道还用代码里的内容");

        var reverted = await PostActionAsync(admin, $"{Root}/{SubmittedKey}/revert", new { channel = "Email", expectedVersion = (long?)null });
        reverted.StatusCode.ShouldBe(HttpStatusCode.OK);
        await SendSubmittedAsync("after-revert", user, "SP-8");

        var delivery = await DeliveryOfAsync(seed, "after-revert");
        delivery.ShouldNotBeNull().ShouldEndWith("|0");
        delivery.ShouldNotContain("新主题");
    }

    [Fact]
    public async Task Detail_ShowsCurrentAndDefaultContent_TheVariables_TheExample_AndTheHistory()
    {
        using var admin = Admin();
        (await PublishAsync(admin, "Email", "新主题", "<p>新正文</p>")).EnsureSuccessStatusCode();

        var detail = await JsonAsync(await admin.GetAsync($"{Root}/{SubmittedKey}", Ct));

        var latest = await LatestVersionAsync(admin);
        var channels = detail.GetProperty("channels").EnumerateArray().ToDictionary(c => c.GetProperty("channel").GetString()!, c => c, StringComparer.Ordinal);
        var email = channels["Email"];
        email.GetProperty("activeVersion").GetInt32().ShouldBe(latest);
        email.GetProperty("subject").GetString().ShouldBe("新主题");
        string.Equals(email.GetProperty("defaultSubject").GetString(), "新主题", StringComparison.Ordinal).ShouldBeFalse();
        channels["InApp"].GetProperty("activeVersion").ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);
        detail.GetProperty("variables").EnumerateArray().Select(v => v.GetString()).Contains("Number", StringComparer.Ordinal).ShouldBeTrue();
        detail.GetProperty("example").GetProperty("Number").GetString().ShouldBe("SP-2026-0001");
        var newest = detail.GetProperty("versions").EnumerateArray().First();
        newest.GetProperty("version").GetInt32().ShouldBe(latest);
        newest.GetProperty("remark").GetString().ShouldBe("测试发布");
    }

    [Fact]
    public async Task APublishWithBrokenSyntax_IsRejected_AndLeavesNothingBehind()
    {
        using var admin = Admin();
        await using var seed = new IdentitySeed(ConnectionString);

        var before = await seed.CountAsync("SELECT count(*) FROM \"Notifications\".\"TemplateVersion\"");

        var response = await PublishAsync(admin, "Email", "主题", "{% if %}<p>坏的</p>");

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await seed.CountAsync("SELECT count(*) FROM \"Notifications\".\"TemplateVersion\"")).ShouldBe(before);
    }

    [Fact]
    public async Task APublishToAChannelTheTemplateDoesNotHave_IsRejected()
    {
        using var admin = Admin();

        var response = await PublishAsync(admin, "Sms", "主题", "正文");

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task APublishWithAStaleVersion_IsAConflict()
    {
        using var admin = Admin();
        var seen = await VersionOfAsync(admin);
        (await PublishAsync(admin, "Email", "第一版", "<p>一</p>", seen)).EnsureSuccessStatusCode();

        var stale = await PublishAsync(admin, "Email", "第二版", "<p>二</p>", seen);

        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Permissions_ViewingNeedsViewAndPublishingNeedsManage()
    {
        using var viewer = Viewer();
        using var anonymous = ClientFor();

        (await viewer.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await PublishAsync(viewer, "Email", "主题", "<p>正文</p>")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await anonymous.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Preview_UsesTheExample_ReportsATemplateErrorInTheBody_AndRefusesRealDataForSecretTemplates()
    {
        using var admin = Admin();

        var response = await PostActionAsync(admin, $"{Root}/{SubmittedKey}/preview", new { channel = "Email", subject = "预览 {{ Number }}", body = "<b>{{ Title }}</b>" });
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Ct));
        var ok = await JsonAsync(response);
        ok.GetProperty("subject").GetString().ShouldBe("预览 SP-2026-0001");
        ok.GetProperty("body").GetString().ShouldBe("<b>第一批样品</b>");
        ok.TryGetProperty("error", out var noError).ShouldBeTrue();
        noError.ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Null);

        var broken = await PostActionAsync(admin, $"{Root}/{SubmittedKey}/preview", new { channel = "Email", subject = "主题", body = "{% if %}" });
        broken.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(broken)).GetProperty("error").GetString().ShouldNotBeNullOrEmpty();

        var secret = await PostActionAsync(admin, $"{Root}/Sample.SpecimenCode/preview", new { channel = "Sms", subject = "验证码", body = "{{ Code }}", model = new { Code = "123456" } });
        secret.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }
}
