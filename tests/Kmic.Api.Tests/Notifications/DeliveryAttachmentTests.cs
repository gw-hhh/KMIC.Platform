using Kmic.Api.Tests.Files;
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
/// 邮件附件：投递记录里存文件 ID，发送时向 Files 取内容；读不到的、太大的按暂时或永久失败记在投递上
/// </summary>
/// <remarks>
/// 邮件渠道换成记录用的，文件走真实的上传和本地存储。附件总大小的上限调成 1 MB，方便测超限
/// </remarks>
public sealed class DeliveryAttachmentTests : FilesApiTestBase, IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly RecordingEmailSender _email = new();
    private static readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "kmic-mail-attachment-tests-" + Guid.CreateVersion7().ToString("N"));

    public DeliveryAttachmentTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
        : base(
            database,
            template,
            withWorker: true,
            storageRoot: _storageRoot,
            moreSettings: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Notifications:Capture:Enabled"] = "false",
                ["Notifications:Delivery:PollSeconds"] = "1",
                ["Notifications:Delivery:MaxAttempts"] = "3",
                ["Notifications:Delivery:MaxAttachmentMb"] = "1",
            },
            configureServices: services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(_email);
            })
    {
        _email.Behavior = (_, _) => SendResult.Sent("fake-email");
    }

    private async Task SendAsync(SendNotification message)
    {
        await using var scope = Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(CallOrigin.Inbox);
        var result = await scope.ServiceProvider.GetRequiredService<ICommandSender>().SendAsync(new ProcessSendNotificationCommand(message, Guid.CreateVersion7(), "Sample", null), Ct);
        result.ShouldBeSuccess();
    }

    private static async Task<Guid> UserAsync(IdentitySeed seed, string name) =>
        await seed.UserAsync($"attach-{name}-{Guid.CreateVersion7():N}"[..40], null, displayName: name, email: $"{Guid.CreateVersion7():N}@example.com");

    private async Task<Guid> UploadAsync(string name, byte[] content)
    {
        using var alice = ClientFor(Alice);
        return await UploadOkAsync(alice, name, content);
    }

    private static Task<string?> StatusAsync(IdentitySeed seed, string dedup) =>
        seed.ScalarAsync<string>($"""SELECT "Status" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' LIMIT 1""");

    private static Task<string?> ErrorAsync(IdentitySeed seed, string dedup) =>
        seed.ScalarAsync<string>($"""SELECT "LastError" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' AND "Channel" = 'Email' LIMIT 1""");

    private static async Task WaitForAsync(IdentitySeed seed, string dedup, string status, string description) =>
        await Eventually.UntilAsync(_ => StatusAsync(seed, dedup), current => string.Equals(current, status, StringComparison.Ordinal), description, TimeSpan.FromSeconds(30), Ct);

    private static SendNotification Notice(Guid user, string dedup, params Guid[] attachments) =>
        SendNotification.For(new SpecimenSubmittedNotice("SP-ATT", "带附件的通知", "/x"), [Recipient.OfUser(user)], dedup, attachments);

    [Fact]
    public async Task AnEmailWithAttachments_IsSentWithTheirNamesAndContent()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "收附件的");
        var first = await UploadAsync("合同.pdf", Pdf("第一份"));
        var second = await UploadAsync("补充.pdf", Pdf("第二份"));
        var dedup = $"att-ok-{Guid.CreateVersion7():N}";

        await SendAsync(Notice(user, dedup, first, second, first));

        await WaitForAsync(seed, dedup, "Sent", "带附件的邮件发出去了");
        var deliveryId = await seed.ScalarAsync<Guid>($"""SELECT "Id" FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' AND "Channel" = 'Email'""");
        var attachments = _email.AttachmentsOf(deliveryId);
        attachments.Select(a => a.Name).ShouldBe(["合同.pdf", "补充.pdf"], "重复的文件 ID 只附一次");
        attachments.Select(a => a.ContentType).ShouldAllBe(type => type == "application/pdf");
        attachments[0].Content.ShouldBe(Pdf("第一份"));
        attachments[1].Content.ShouldBe(Pdf("第二份"));
    }

    [Fact]
    public async Task AnAttachmentThatDoesNotExist_FailsTheDeliveryForGood_WithoutCallingTheChannel()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "附件没了的");
        var dedup = $"att-missing-{Guid.CreateVersion7():N}";
        var before = _email.Sent.Count;

        await SendAsync(Notice(user, dedup, Guid.CreateVersion7()));

        await WaitForAsync(seed, dedup, "Failed", "附件不存在，投递失败");
        (await ErrorAsync(seed, dedup)).ShouldBe("附件不存在或已经被删除");
        _email.Sent.Skip(before).ShouldNotContain(m => m.Subject.Contains("SP-ATT", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AttachmentsOverTheSizeLimit_FailTheDeliveryForGood()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "附件太大的");
        var big = await UploadAsync("大文件.pdf", [.. Pdf("大"), .. new byte[700 * 1024]]);
        var another = await UploadAsync("另一个大文件.pdf", [.. Pdf("也大"), .. new byte[700 * 1024]]);
        var dedup = $"att-big-{Guid.CreateVersion7():N}";

        await SendAsync(Notice(user, dedup, big, another));

        await WaitForAsync(seed, dedup, "Failed", "附件加起来超过上限，投递失败");
        (await ErrorAsync(seed, dedup)).ShouldBe("附件加起来超过了 1 MB");
    }

    [Fact]
    public async Task AnAttachmentThatIsNotReadyYet_BacksOffAndIsSentOnceItIs()
    {
        await using var seed = new IdentitySeed(ConnectionString);
        var user = await UserAsync(seed, "附件还没好的");
        var file = await UploadAsync("稍后.pdf", Pdf("稍后"));
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "Status" = 'Uploading' WHERE "Id" = @id""", ("id", file));
        var dedup = $"att-later-{Guid.CreateVersion7():N}";

        await SendAsync(Notice(user, dedup, file));

        await Eventually.UntilAsync(
            _ => seed.ScalarAsync<bool>($"""SELECT "Status" = 'Pending' AND "LastError" = '附件暂时读不到' AND "NextAttemptAt" > now() FROM "Notifications"."Delivery" WHERE "DedupKey" = '{dedup}' AND "Channel" = 'Email'"""),
            backedOff => backedOff,
            "附件还没准备好，投递放回队列并退避",
            TimeSpan.FromSeconds(30),
            Ct);
        await seed.SqlAsync("""UPDATE "Files"."StoredFile" SET "Status" = 'Available' WHERE "Id" = @id""", ("id", file));
        await seed.SqlAsync($"""UPDATE "Notifications"."Delivery" SET "NextAttemptAt" = now() WHERE "DedupKey" = '{dedup}'""");

        await WaitForAsync(seed, dedup, "Sent", "附件准备好之后发出去了");
    }
}
