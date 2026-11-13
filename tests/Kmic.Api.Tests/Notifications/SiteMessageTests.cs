using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Notifications.Application.SiteMessages.Purge;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 站内消息：只有自己的；未读在前；已读、全部已读；别人的消息和不存在的一样
/// </summary>
public sealed class SiteMessageTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/notifications/site-messages";

    private static async Task<Guid> InsertAsync(IdentitySeed seed, Guid recipient, string title, string category = "Sample", string? link = "/sample/specimens/1", int minutesAgo = 0, bool read = false)
    {
        var id = Guid.CreateVersion7();
        await seed.SqlAsync(
            """
            INSERT INTO "Notifications"."SiteMessage" ("Id", "RecipientUserId", "Title", "Body", "Link", "Category", "SentAt", "ReadAt", "CreatedAt", "CreatedBy", "ConcurrencyVersion")
            VALUES (@id, @recipient, @title, '正文', @link, @category, now() - make_interval(mins => @minutes), CASE WHEN @read THEN now() ELSE NULL END, now(), @recipient, 0)
            """,
            ("id", id), ("recipient", recipient), ("title", title), ("link", (object?)link ?? DBNull.Value), ("category", category), ("minutes", minutesAgo), ("read", read));
        return id;
    }

    [Fact]
    public async Task List_ShowsOnlyMyMessages_UnreadFirst_AndNewestFirstWithinEach()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var tag = Guid.CreateVersion7().ToString("N")[..8];
        await InsertAsync(seed, Alice.UserId.Value, $"{tag}-已读较新", minutesAgo: 1, read: true);
        await InsertAsync(seed, Alice.UserId.Value, $"{tag}-未读较旧", minutesAgo: 30);
        await InsertAsync(seed, Alice.UserId.Value, $"{tag}-未读较新", minutesAgo: 5);
        await InsertAsync(seed, Bob.UserId.Value, $"{tag}-乙的消息");
        using var alice = ClientFor(Alice);

        var list = await JsonAsync(await alice.GetAsync($"{Root}?pageSize=100", Ct));

        var titles = list.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()!).Where(t => t.StartsWith(tag, StringComparison.Ordinal)).ToList();
        titles.ShouldBe([$"{tag}-未读较新", $"{tag}-未读较旧", $"{tag}-已读较新"]);
    }

    [Fact]
    public async Task List_CanBeFilteredByReadStateAndCategory()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var tag = Guid.CreateVersion7().ToString("N")[..8];
        await InsertAsync(seed, Carol.UserId.Value, $"{tag}-未读审批", "Approval");
        await InsertAsync(seed, Carol.UserId.Value, $"{tag}-已读审批", "Approval", read: true);
        await InsertAsync(seed, Carol.UserId.Value, $"{tag}-未读系统", "System");
        using var carol = ClientFor(Carol);

        var unread = await JsonAsync(await carol.GetAsync($"{Root}?unread=true&category=Approval", Ct));
        var read = await JsonAsync(await carol.GetAsync($"{Root}?unread=false&category=Approval", Ct));

        unread.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()).ShouldBe([$"{tag}-未读审批"]);
        read.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()).ShouldBe([$"{tag}-已读审批"]);
    }

    [Fact]
    public async Task UnreadCount_CountsOnlyMyUnreadMessages()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        using var alice = ClientFor(Alice);
        var before = (await JsonAsync(await alice.GetAsync($"{Root}/unread-count", Ct))).GetProperty("count").GetInt32();
        await InsertAsync(seed, Alice.UserId.Value, "未读 1");
        await InsertAsync(seed, Alice.UserId.Value, "未读 2");
        await InsertAsync(seed, Alice.UserId.Value, "已读", read: true);
        await InsertAsync(seed, Bob.UserId.Value, "乙的未读");

        var after = (await JsonAsync(await alice.GetAsync($"{Root}/unread-count", Ct))).GetProperty("count").GetInt32();

        after.ShouldBe(before + 2);
    }

    [Fact]
    public async Task MarkRead_ChangesTheStateOnce_AndAnotherMarkIsHarmless()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var id = await InsertAsync(seed, Alice.UserId.Value, "要标已读的");
        using var alice = ClientFor(Alice);

        var first = await alice.PostAsync($"{Root}/{id}/read", null, Ct);
        var readAt = await seed.ScalarAsync<DateTime>("""SELECT "ReadAt" FROM "Notifications"."SiteMessage" WHERE "Id" = @id""", ("id", id));
        var second = await alice.PostAsync($"{Root}/{id}/read", null, Ct);

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await seed.ScalarAsync<DateTime>("""SELECT "ReadAt" FROM "Notifications"."SiteMessage" WHERE "Id" = @id""", ("id", id))).ShouldBe(readAt, "第二次不改已读时间");
    }

    [Fact]
    public async Task MarkRead_OfSomeoneElsesMessage_IsNotFound_AndDoesNotChangeIt()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var theirs = await InsertAsync(seed, Bob.UserId.Value, "乙的消息");
        using var alice = ClientFor(Alice);

        var response = await alice.PostAsync($"{Root}/{theirs}/read", null, Ct);
        var missing = await alice.PostAsync($"{Root}/{Guid.CreateVersion7()}/read", null, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Notifications.SiteMessageNotFound");
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await seed.ScalarAsync<bool>("""SELECT "ReadAt" IS NULL FROM "Notifications"."SiteMessage" WHERE "Id" = @id""", ("id", theirs))).ShouldBeTrue();
    }

    [Fact]
    public async Task ReadAll_MarksOnlyMyUnreadMessages()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var mine1 = await InsertAsync(seed, Carol.UserId.Value, "我的 1");
        var mine2 = await InsertAsync(seed, Carol.UserId.Value, "我的 2");
        var theirs = await InsertAsync(seed, Bob.UserId.Value, "乙的");
        using var carol = ClientFor(Carol);

        var response = await carol.PostAsync($"{Root}/read-all", null, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(response)).GetProperty("count").GetInt32().ShouldBeGreaterThanOrEqualTo(2);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."SiteMessage" WHERE "Id" IN ('{mine1}', '{mine2}') AND "ReadAt" IS NOT NULL""")).ShouldBe(2);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."SiteMessage" WHERE "Id" = '{theirs}' AND "ReadAt" IS NULL""")).ShouldBe(1);
        (await JsonAsync(await carol.PostAsync($"{Root}/read-all", null, Ct))).GetProperty("count").GetInt32().ShouldBe(0, "没有未读的了");
    }

    [Fact]
    public async Task Purge_RemovesOldReadAndVeryOldUnreadMessages_ButKeepsTheRest()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var oldRead = await InsertAsync(seed, Alice.UserId.Value, "读了很久", read: true);
        var recentRead = await InsertAsync(seed, Alice.UserId.Value, "刚读过", read: true);
        var oldUnread = await InsertAsync(seed, Alice.UserId.Value, "一年多没读");
        var recentUnread = await InsertAsync(seed, Alice.UserId.Value, "三个月没读");
        await seed.SqlAsync("""UPDATE "Notifications"."SiteMessage" SET "ReadAt" = now() - interval '100 days' WHERE "Id" = @id""", ("id", oldRead));
        await seed.SqlAsync("""UPDATE "Notifications"."SiteMessage" SET "ReadAt" = now() - interval '10 days' WHERE "Id" = @id""", ("id", recentRead));
        await seed.SqlAsync("""UPDATE "Notifications"."SiteMessage" SET "SentAt" = now() - interval '400 days' WHERE "Id" = @id""", ("id", oldUnread));
        await seed.SqlAsync("""UPDATE "Notifications"."SiteMessage" SET "SentAt" = now() - interval '100 days' WHERE "Id" = @id""", ("id", recentUnread));

        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.ScheduledJob);
        var outcome = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new PurgeSiteMessagesCommand(TimeProvider.System.GetUtcNow()), Ct);

        outcome.ShouldBeSuccess().ItemsProcessed.ShouldBeGreaterThanOrEqualTo(2);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."SiteMessage" WHERE "Id" IN ('{oldRead}', '{oldUnread}')""")).ShouldBe(0);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."SiteMessage" WHERE "Id" IN ('{recentRead}', '{recentUnread}')""")).ShouldBe(2);
    }

    [Fact]
    public async Task Endpoints_NeedLogin()
    {
        using var anonymous = ClientFor();

        (await anonymous.GetAsync(Root, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await anonymous.GetAsync($"{Root}/unread-count", Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await anonymous.PostAsync($"{Root}/read-all", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
