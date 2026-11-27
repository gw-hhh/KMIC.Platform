using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

public sealed class NotificationEndToEndTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template, new Dictionary<string, string>(StringComparer.Ordinal) { ["Kmic:Roles"] = "Api,Worker" }),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    [Fact]
    public async Task SubmittingASpecimen_NotifiesItsOwner_ByEmailAndSiteMessage()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        await seed.UserAsync($"owner-{Guid.CreateVersion7():N}"[..30], null, displayName: "标本负责人", email: "owner@example.com", id: Alice.UserId.Value);
        using var alice = ClientFor(Alice);
        var (specimen, version) = await CreateAsync(alice, "要提交的标本");

        var submitted = await PostActionAsync(alice, $"/api/v1/sample/specimens/{specimen}/submit", new { expectedVersion = version });
        submitted.EnsureSuccessStatusCode();

        var dedup = $"Sample.SpecimenSubmitted:{specimen}";
        await Eventually.UntilAsync(
            _ => seed.CountAsync($"""SELECT count(*) FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' AND "Status" = 'Captured' AND "AddressMasked" = 'o***@example.com'"""),
            count => count == 1,
            "邮件经发件箱、路由与收件箱处理后生成已捕获的投递记录",
            TimeSpan.FromSeconds(30),
            Ct);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."SiteMessage" WHERE "RecipientUserId" = '{Alice.UserId.Value}' AND "Link" = '/sample/specimens/{specimen}'""")).ShouldBe(1);
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."Notification" WHERE "DedupKey" = '{dedup}' AND "SourceModule" = 'Sample'""")).ShouldBe(1);
    }
}
