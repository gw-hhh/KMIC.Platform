using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Notifications.Application.Channels;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Contracts;
using Kmic.Sample.Application.Notifications;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 投递队列：领取、发送、记录结果；暂时失败退避重试，永久失败不重试，含密通知有有效期、发完清空正文
/// </summary>
public sealed class DeliveryTests : ApiTestBase, IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly RecordingEmailSender _email = new();
    private static readonly RecordingSmsGateway _sms = new();

    public DeliveryTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
        : base(
            database,
            template,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Kmic:Roles"] = "Api,Worker",
                ["Notifications:Capture:Enabled"] = "false",
                ["Notifications:Delivery:PollSeconds"] = "1",
                ["Notifications:Delivery:MaxAttempts"] = "3",
            },
            services =>
            {
                services.RemoveAll<IEmailSender>();
                services.RemoveAll<ISmsGateway>();
                services.AddSingleton<IEmailSender>(_email);
                services.AddSingleton<ISmsGateway>(_sms);
            })
    {
        _email.Behavior = (_, _) => SendResult.Sent("fake-email");
        _sms.Behavior = (_, _) => SendResult.Sent("fake-sms");
    }

    private async Task SendAsync(SendNotification message)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private async Task SendSecretAsync(SendSecretNotification message)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendSecretNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private static async Task<Guid> UserAsync(IdentitySeed seed, string name, string? email = null, string? mobile = null, bool active = true) =>
        await seed.UserAsync($"deliver-{name}-{Guid.CreateVersion7():N}"[..40], null, displayName: name, mobile: mobile, active: active, email: email);

    private static async Task<string?> StatusAsync(IdentitySeed seed, string dedup) =>
        await seed.ScalarAsync<string>($"""SELECT "Status" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' LIMIT 1""");

    private static async Task WaitForStatusAsync(IdentitySeed seed, string dedup, string status) =>
        await Eventually.UntilAsync(
            _ => StatusAsync(seed, dedup),
            current => string.Equals(current, status, StringComparison.Ordinal),
            $"投递变成 {status}",
            TimeSpan.FromSeconds(30),
            Ct);

    [Fact]
    public async Task AnEmailToAUser_IsSentToTheAddressIdentityHasNow_AndRecorded()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "收邮件的", email: "now@example.com");
        var dedup = $"send-{Guid.CreateVersion7():N}";

        await SendAsync(SendNotification.For(new SpecimenSubmittedNotice("SP-1", "标题", "/x"), [Recipient.OfUser(user)], dedup));

        await WaitForStatusAsync(seed, dedup, "Sent");
        var mail = _email.Sent.Last(m => m.Subject.Contains("SP-1", StringComparison.Ordinal));
        mail.Address.ShouldBe("now@example.com", "用户的邮箱在发送时向 Identity 取，不存进投递记录");
        mail.Role.ShouldBe("To");
        mail.SenderProfile.ShouldBe("Default");
        (await seed.ScalarAsync<bool>($"""SELECT "Address" IS NULL AND "SentAt" IS NOT NULL AND "ProviderMessageId" = 'fake-email' AND "Attempts" = 1 FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBeTrue();
    }

    [Fact]
    public async Task ATransientFailure_BacksOffAndIsRetried_UntilItSucceeds()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "重试的", email: "retry@example.com");
        var dedup = $"retry-{Guid.CreateVersion7():N}";
        var attempts = 0;
        _email.Behavior = (message, _) => message.Subject.Contains("SP-RETRY", StringComparison.Ordinal) && Interlocked.Increment(ref attempts) == 1 ? SendResult.Transient("邮箱忙") : SendResult.Sent("ok");

        await SendAsync(SendNotification.For(new SpecimenSubmittedNotice("SP-RETRY", "标题", "/x"), [Recipient.OfUser(user)], dedup));

        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<bool>($"""SELECT "Status" = 'Pending' AND "LastError" = '邮箱忙' AND "NextAttemptAt" > now() AND "Attempts" = 1 FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'"""),
            backedOff => backedOff,
            "第一次暂时失败，放回队列并退避",
            TimeSpan.FromSeconds(30),
            Ct);
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "NextAttemptAt" = now() WHERE "DedupKey" = '{dedup}'""");

        await WaitForStatusAsync(seed, dedup, "Sent");
        (await seed.ScalarAsync<int>($"""SELECT "Attempts" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBe(2);
    }

    [Fact]
    public async Task APermanentFailure_IsNotRetried_AndKeepsTheReason()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "地址不存在的", email: "nobody@example.com");
        var dedup = $"permanent-{Guid.CreateVersion7():N}";
        _email.Behavior = (message, _) => message.Subject.Contains("SP-PERM", StringComparison.Ordinal) ? SendResult.Permanent("邮箱不存在") : SendResult.Sent("ok");

        await SendAsync(SendNotification.For(new SpecimenSubmittedNotice("SP-PERM", "标题", "/x"), [Recipient.OfUser(user)], dedup));

        await WaitForStatusAsync(seed, dedup, "Failed");
        (await seed.ScalarAsync<bool>($"""SELECT "LastError" = '邮箱不存在' AND "Attempts" = 1 FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBeTrue();
    }

    [Fact]
    public async Task WhenAttemptsRunOut_ATransientFailureBecomesFailed()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "一直失败的", email: "always@example.com");
        var dedup = $"exhaust-{Guid.CreateVersion7():N}";
        _email.Behavior = (message, _) => message.Subject.Contains("SP-EXHAUST", StringComparison.Ordinal) ? SendResult.Transient("一直忙") : SendResult.Sent("ok");

        await SendAsync(SendNotification.For(new SpecimenSubmittedNotice("SP-EXHAUST", "标题", "/x"), [Recipient.OfUser(user)], dedup));

        for (var round = 1; round <= 2; round++)
        {
            var expected = round;
            await Eventually.UntilAsync(
                _ => seed.ScalarAsync<bool>($"""SELECT "Status" = 'Pending' AND "Attempts" = {expected} FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'"""),
                waiting => waiting,
                $"第 {round} 次失败后退避",
                TimeSpan.FromSeconds(30),
                Ct);
            await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "NextAttemptAt" = now() WHERE "DedupKey" = '{dedup}'""");
        }

        await WaitForStatusAsync(seed, dedup, "Failed");
        (await seed.ScalarAsync<int>($"""SELECT "Attempts" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBe(3, "配置的最多 3 次");
    }

    [Fact]
    public async Task ARecipientDisabledAfterwards_IsSkippedAtSendTime()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "后来被禁用的", email: "late@example.com");
        var dedup = $"late-{Guid.CreateVersion7():N}";
        _email.Behavior = (message, _) => message.Subject.Contains("SP-LATE", StringComparison.Ordinal) ? SendResult.Transient("先别发") : SendResult.Sent("ok");
        await SendAsync(SendNotification.For(new SpecimenSubmittedNotice("SP-LATE", "标题", "/x"), [Recipient.OfUser(user)], dedup));
        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<bool>($"""SELECT "Status" = 'Pending' AND "Attempts" = 1 FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'"""),
            waiting => waiting,
            "第一次没发出去，在队列里等",
            TimeSpan.FromSeconds(30),
            Ct);

        await seed.SqlAsync("""UPDATE "Identity"."User" SET "Status" = 'Disabled' WHERE "Id" = @id""", ("id", user));
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "NextAttemptAt" = now() WHERE "DedupKey" = '{dedup}'""");

        await WaitForStatusAsync(seed, dedup, "Skipped");
        (await seed.ScalarAsync<string>($"""SELECT "SkipReason" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBe("UserDisabled");
    }

    [Fact]
    public async Task ASecretSms_IsSentWithItsParameters_AndTheBodyIsClearedAfterwards()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "收验证码的", mobile: "+8613900000001");
        var dedup = $"secret-{Guid.CreateVersion7():N}";

        await SendSecretAsync(SendSecretNotification.For(new SpecimenCode("654321"), [Recipient.OfUser(user)], dedup));

        await WaitForStatusAsync(seed, dedup, "Sent");
        var sms = _sms.Sent.Last(m => string.Equals(m.Phone, "+8613900000001", StringComparison.Ordinal));
        sms.TemplateKey.ShouldBe("Sample.SpecimenCode");
        sms.Parameters["code"].ShouldBe("654321");
        (await seed.ScalarAsync<bool>($"""SELECT "Body" IS NULL AND "SmsParameters" IS NULL AND "Priority" = 'High' FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}'""")).ShouldBeTrue("验证码发完就不再留在库里");
    }
}
