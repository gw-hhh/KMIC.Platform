using System.Net.Sockets;
using Kmic.Notifications.Application.Channels;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>
/// <see cref="IEmailSender" /> 的实现：MailKit 经 SMTP 发送
/// </summary>
/// <remarks>
/// 每封邮件开一个连接，发完关闭；邮件头 Message-ID 由投递 ID 生成，重试发的是同一个标识，收件端可以据此去重。
/// 失败分类：服务器返回 4xx、网络和超时、认证失败（配置修好之后重试能成功）是暂时的；5xx、地址格式不对、没有配置发件账号是永久的
/// </remarks>
internal sealed class MailKitEmailSender(IOptions<NotificationsEmailOptions> options) : IEmailSender
{
    public async Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!options.Value.Senders.TryGetValue(message.SenderProfile, out var sender) || string.IsNullOrWhiteSpace(sender.Host))
        {
            return SendResult.Permanent($"没有配置发件账号 {message.SenderProfile}");
        }

        MimeMessage mime;
        try
        {
            mime = Build(message, sender);
        }
        catch (ParseException)
        {
            return SendResult.Permanent("收件地址格式不对");
        }

        try
        {
            using var client = new SmtpClient { Timeout = options.Value.TimeoutSeconds * 1000 };
            await client.ConnectAsync(sender.Host, sender.Port, SecurityOf(sender.Security), ct);
            if (!string.IsNullOrEmpty(sender.Username))
            {
                await client.AuthenticateAsync(sender.Username, sender.Password, ct);
            }

            await client.SendAsync(mime, ct);
            await client.DisconnectAsync(quit: true, ct);
            return SendResult.Sent(mime.MessageId);
        }
        catch (SmtpCommandException exception)
        {
            // 4xx 是暂时的（邮箱忙、稍后再试），5xx 是永久的（邮箱不存在、被拒绝）
            return (int)exception.StatusCode >= 500 ? SendResult.Permanent($"服务器拒绝：{(int)exception.StatusCode}") : SendResult.Transient($"服务器暂时不可用：{(int)exception.StatusCode}");
        }
        catch (Exception exception) when (exception is AuthenticationException or SaslException or ProtocolException or ServiceNotConnectedException or ServiceNotAuthenticatedException or IOException or SocketException or TimeoutException or OperationCanceledException)
        {
            // 网络、协议、超时、认证：配置或网络恢复之后重试能成功
            return SendResult.Transient($"连接或认证失败：{exception.GetType().Name}");
        }
    }

    private static MimeMessage Build(EmailMessage message, EmailSenderOptions sender)
    {
        var mime = new MimeMessage { Subject = message.Subject, MessageId = $"{message.DeliveryId:D}@kmic" };
        mime.From.Add(new MailboxAddress(sender.DisplayName, sender.Address));
        var recipient = MailboxAddress.Parse(message.Address);
        switch (message.Role)
        {
            case "Cc":
                mime.Cc.Add(recipient);
                break;
            case "Bcc":
                mime.Bcc.Add(recipient);
                break;
            default:
                mime.To.Add(recipient);
                break;
        }

        mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();
        return mime;
    }

    private static SecureSocketOptions SecurityOf(string security) =>
        security switch
        {
            "SslOnConnect" => SecureSocketOptions.SslOnConnect,
            "None" => SecureSocketOptions.None,
            _ => SecureSocketOptions.StartTls,
        };
}
