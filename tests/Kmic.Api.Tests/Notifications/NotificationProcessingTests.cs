using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

public sealed class NotificationProcessingTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private async Task<Result> SendAsync(SendNotification message, Guid? sourceMessageId = null)
    {
        await using var scope = Factory.Services.AsSystem(CallOrigin.Inbox);
        return await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendNotificationCommand(message, sourceMessageId ?? Guid.CreateVersion7(), "Sample", null), Ct);
    }

    private async Task<Result> SendSecretAsync(SendSecretNotification message)
    {
        await using var scope = Factory.Services.AsSystem(CallOrigin.Inbox);
        return await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendSecretNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
    }

    private static SendNotification Submitted(string dedup, params Recipient[] to) =>
        SendNotification.For(new SpecimenSubmittedNotice("SP-<1>", "第一批 & 样品", "/sample/specimens/1"), to, dedup);

    private static async Task<Guid> UserAsync(IdentitySeed seed, string name, string? email = null, string? mobile = null, bool active = true) =>
        await seed.UserAsync($"notify-{name}-{Guid.CreateVersion7():N}"[..40], null, displayName: name, mobile: mobile, active: active, email: email);

    [Fact]
    public async Task AnEmailAndInAppTemplate_ToAUser_GivesACapturedEmailAndASiteMessage()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "收件人", email: "alice@example.com");
        var dedup = $"submitted-{Guid.CreateVersion7():N}";

        (await SendAsync(Submitted(dedup, Recipient.OfUser(user)))).ShouldBeSuccess();

        var delivery = await seed.ScalarAsync<string>($"""SELECT "Status" || '|' || "AddressMasked" || '|' || "Subject" || '|' || "Body" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""");
        delivery.ShouldBe("Captured|a***@example.com|示例 SP-<1> 已提交|<p>示例 SP-&lt;1&gt;（第一批 &amp; 样品）已提交。</p><p><a href=\"/sample/specimens/1\">查看详情</a></p>");
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."CapturedMessage" c JOIN "Notifications"."Delivery" d ON d."Id" = c."DeliveryId" WHERE d."DedupKey" = '{dedup}' AND c."Address" = 'alice@example.com'""")).ShouldBe(1);
        var site = await seed.ScalarAsync<string>($"""SELECT "Title" || '|' || "Body" || '|' || "Link" || '|' || "Category" FROM "Notifications"."SiteMessage" WHERE "RecipientUserId" = '{user}'""");
        site.ShouldBe("示例 SP-<1> 已提交|示例 SP-<1>（第一批 & 样品）已提交。|/sample/specimens/1|Sample");
    }

    [Fact]
    public async Task ARecipientWithoutAnAddress_OrDisabled_OrUnknown_GetsASkippedRecordWithTheReason()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var noEmail = await UserAsync(seed, "没有邮箱");
        var disabled = await UserAsync(seed, "已禁用", email: "gone@example.com", active: false);
        var unknown = Guid.CreateVersion7();
        var dedup = $"skip-{Guid.CreateVersion7():N}";

        (await SendAsync(Submitted(dedup, Recipient.OfUser(noEmail), Recipient.OfUser(disabled), Recipient.OfUser(unknown)))).ShouldBeSuccess();

        async Task<string?> ReasonOfAsync(Guid user) =>
            await seed.ScalarAsync<string>($"""SELECT "Status" || ':' || "SkipReason" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' AND "RecipientUserId" = '{user}'""");
        (await ReasonOfAsync(noEmail)).ShouldBe("Skipped:NoAddress");
        (await ReasonOfAsync(disabled)).ShouldBe("Skipped:UserDisabled");
        (await ReasonOfAsync(unknown)).ShouldBe("Skipped:UnknownUser");
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."SiteMessage" WHERE "RecipientUserId" IN ('{disabled}', '{unknown}')""")).ShouldBe(0, "已禁用或不存在的用户不应生成站内消息");
    }

    [Fact]
    public async Task ARecipientGivenByAddress_KeepsTheAddress_AndIsKeyedByItsHash()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var dedup = $"direct-{Guid.CreateVersion7():N}";

        (await SendAsync(Submitted(dedup, Recipient.OfEmail("Customer@Example.com", RecipientRole.Cc)))).ShouldBeSuccess();

        var row = await seed.ScalarAsync<string>($"""SELECT "Address" || '|' || "AddressMasked" || '|' || "Role" || '|' || left("RecipientKey", 2) || '|' || ("RecipientUserId" IS NULL) FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""");
        row.ShouldBe("Customer@Example.com|C***@Example.com|Cc|a:|true");
    }

    [Fact]
    public async Task ASmsTemplate_GivesAMaskedSmsDelivery()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "短信收件人", mobile: "+8613800000000");
        var dedup = $"sms-{Guid.CreateVersion7():N}";
        var message = SendNotification.For(new SpecimenAlertSms("SP-9"), [Recipient.OfUser(user)], dedup);

        (await SendAsync(message)).ShouldBeSuccess();

        var row = await seed.ScalarAsync<string>($"""SELECT "Channel" || '|' || "AddressMasked" || '|' || "Body" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""");
        row.ShouldBe("Sms|+86*******0000|示例 SP-9 需要处理");
    }

    [Fact]
    public async Task TheSameSourceMessage_IsProcessedOnce_AndTheSameDedupKeyIsNotSentTwice()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "只收一次", email: "once@example.com");
        var dedup = $"once-{Guid.CreateVersion7():N}";
        var source = Guid.CreateVersion7();

        (await SendAsync(Submitted(dedup, Recipient.OfUser(user)), source)).ShouldBeSuccess();
        (await SendAsync(Submitted(dedup, Recipient.OfUser(user)), source)).ShouldBeSuccess();
        (await SendAsync(Submitted(dedup, Recipient.OfUser(user)))).ShouldBeSuccess();

        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBe(1, "相同去重键、收件人与渠道组合仅应生成一条投递记录");
        (await seed.CountAsync($"""SELECT count(*) FROM "Notifications"."Notification" WHERE "SourceMessageId" = '{source}'""")).ShouldBe(1, "同一来源消息仅应处理一次");
    }

    [Fact]
    public async Task ATemplateThatDoesNotExist_AndASecretMismatch_AreBusinessFailures()
    {
        var unknown = new SendNotification("Sample.NoSuchTemplate", [Recipient.OfUser(Guid.CreateVersion7())], System.Text.Json.JsonDocument.Parse("{}").RootElement, "x");
        var secretAsPlain = new SendNotification("Sample.SpecimenCode", [Recipient.OfUser(Guid.CreateVersion7())], System.Text.Json.JsonDocument.Parse("""{"Code":"1"}""").RootElement, "x");
        var plainAsSecret = SendSecretNotification.For(new SpecimenCode("123456"), [Recipient.OfPhone("+8613800000001")], "x");
        var wrongShape = new SendNotification("Sample.SpecimenSubmitted", [Recipient.OfUser(Guid.CreateVersion7())], System.Text.Json.JsonDocument.Parse("""{"Unexpected":1}""").RootElement, "x");

        (await SendAsync(unknown)).ShouldBeFailure(Kmic.Notifications.Domain.Errors.NotificationsErrors.TemplateNotFound);
        (await SendAsync(secretAsPlain)).ShouldBeFailure(Kmic.Notifications.Domain.Errors.NotificationsErrors.TemplateSecretMismatch);
        (await SendAsync(wrongShape)).IsFailure.ShouldBeTrue();
        (await SendSecretAsync(plainAsSecret)).ShouldBeSuccess();
    }
}
