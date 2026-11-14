using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Notifications.Application.Channels;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 邮件渠道（MailKit）：邮件怎么发出去，各种失败怎么分成暂时和永久。SMTP 服务器用测试里的假服务器
/// </summary>
public sealed class MailKitEmailSenderTests : ApiTestBase, IClassFixture<ClassDatabase<ApiTemplate>>, IClassFixture<StubSmtpServer>
{
    private readonly StubSmtpServer _smtp;

    public MailKitEmailSenderTests(ClassDatabase<ApiTemplate> database, ApiTemplate template, StubSmtpServer smtp)
        : base(database, template, Settings(smtp))
    {
        _smtp = smtp;
        _smtp.Reset();
    }

    private IEmailSender Sender => Factory.Services.GetRequiredService<IEmailSender>();

    // Default 连假服务器；Down 指向一个没有人监听的端口，用来模拟连不上
    private static Dictionary<string, string> Settings(StubSmtpServer smtp) =>
        new(StringComparer.Ordinal)
        {
            ["Notifications:Email:Senders:Default:Address"] = "noreply@example.com",
            ["Notifications:Email:Senders:Default:DisplayName"] = "KMIC",
            ["Notifications:Email:Senders:Default:Host"] = "127.0.0.1",
            ["Notifications:Email:Senders:Default:Port"] = smtp.Port.ToString(CultureInfo.InvariantCulture),
            ["Notifications:Email:Senders:Default:Security"] = "None",
            ["Notifications:Email:Senders:Down:Address"] = "noreply@example.com",
            ["Notifications:Email:Senders:Down:Host"] = "127.0.0.1",
            ["Notifications:Email:Senders:Down:Port"] = UnusedPort().ToString(CultureInfo.InvariantCulture),
            ["Notifications:Email:Senders:Down:Security"] = "None",
            ["Notifications:Email:TimeoutSeconds"] = "5",
        };

    private static int UnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static EmailMessage Message(string address = "to@example.com", string role = "To", string profile = "Default") =>
        new(Guid.CreateVersion7(), profile, address, role, "Hello", "<p>hello</p>", []);

    [Fact]
    public async Task AMail_IsSentWithAMessageIdMadeFromTheDeliveryId()
    {
        var message = Message();

        var result = await Sender.SendAsync(message, Ct);

        result.Outcome.ShouldBe(SendOutcome.Sent);
        result.ProviderMessageId.ShouldBe($"{message.DeliveryId:D}@kmic");
        var mail = _smtp.Mails.ShouldHaveSingleItem();
        mail.From.ShouldBe("noreply@example.com");
        mail.Recipients.ShouldBe(["to@example.com"]);
        mail.Data.ShouldContain($"Message-ID: <{message.DeliveryId:D}@kmic>");
        mail.Data.ShouldContain("Subject: Hello");
        mail.Data.ShouldContain("Content-Type: text/html");
    }

    [Fact]
    public async Task AnAttachment_IsSentAsABase64PartWithItsFileName()
    {
        var content = Encoding.ASCII.GetBytes("%PDF-1.7 attachment body");
        await using var stream = new MemoryStream(content);
        var message = Message() with { Attachments = [new EmailAttachment("contract.pdf", "application/pdf", stream)] };

        var result = await Sender.SendAsync(message, Ct);

        result.Outcome.ShouldBe(SendOutcome.Sent);
        var mail = _smtp.Mails.ShouldHaveSingleItem();
        mail.Data.ShouldContain("multipart/mixed");
        mail.Data.ShouldContain("Content-Type: application/pdf");
        mail.Data.ShouldContain("contract.pdf");
        mail.Data.ShouldContain(Convert.ToBase64String(content));
    }

    [Fact]
    public async Task ACcRecipient_IsShownInTheHeaders()
    {
        var result = await Sender.SendAsync(Message("cc@example.com", role: "Cc"), Ct);

        result.Outcome.ShouldBe(SendOutcome.Sent);
        var mail = _smtp.Mails.ShouldHaveSingleItem();
        mail.Recipients.ShouldBe(["cc@example.com"]);
        mail.Data.ShouldContain("Cc: cc@example.com");
    }

    [Fact]
    public async Task ABccRecipient_ReceivesTheMail_ButIsNotInTheHeaders()
    {
        var result = await Sender.SendAsync(Message("bcc@example.com", role: "Bcc"), Ct);

        result.Outcome.ShouldBe(SendOutcome.Sent);
        var mail = _smtp.Mails.ShouldHaveSingleItem();
        mail.Recipients.ShouldBe(["bcc@example.com"]);
        mail.Data.ShouldNotContain("bcc@example.com");
    }

    [Fact]
    public async Task ARecipientTheServerRefuses_IsAPermanentFailure()
    {
        _smtp.RcptReply = "550 no such user";

        var result = await Sender.SendAsync(Message(), Ct);

        result.Outcome.ShouldBe(SendOutcome.Permanent);
        result.Error.ShouldNotBeNull().ShouldContain("550");
        _smtp.Mails.ShouldBeEmpty();
    }

    [Fact]
    public async Task AServerThatIsBusy_IsATransientFailure()
    {
        _smtp.RcptReply = "450 mailbox busy";

        var result = await Sender.SendAsync(Message(), Ct);

        result.Outcome.ShouldBe(SendOutcome.Transient);
        result.Error.ShouldNotBeNull().ShouldContain("450");
    }

    [Fact]
    public async Task AServerThatCannotBeReached_IsATransientFailure()
    {
        var result = await Sender.SendAsync(Message(profile: "Down"), Ct);

        result.Outcome.ShouldBe(SendOutcome.Transient);
    }

    [Fact]
    public async Task AnAddressThatIsNotAnAddress_IsAPermanentFailure_AndNothingIsSent()
    {
        var result = await Sender.SendAsync(Message("not an address@@"), Ct);

        result.Outcome.ShouldBe(SendOutcome.Permanent);
        _smtp.Mails.ShouldBeEmpty();
    }

    [Fact]
    public async Task ASenderProfileThatIsNotConfigured_IsAPermanentFailure()
    {
        var result = await Sender.SendAsync(Message(profile: "Missing"), Ct);

        result.Outcome.ShouldBe(SendOutcome.Permanent);
        result.Error.ShouldNotBeNull().ShouldContain("Missing");
    }
}
