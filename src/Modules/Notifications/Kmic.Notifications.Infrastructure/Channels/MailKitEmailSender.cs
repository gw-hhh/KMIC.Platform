using System.Net.Sockets;
using Kmic.Notifications.Application.Channels;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Kmic.Notifications.Infrastructure.Channels;

internal sealed class MailKitEmailSender(IOptions<NotificationsEmailOptions> options) : IEmailSender
{
    public async Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!options.Value.Senders.TryGetValue(message.SenderProfile, out var sender) || string.IsNullOrWhiteSpace(sender.Host))
        {
            return SendResult.Permanent($"未配置发件账号 {message.SenderProfile}");
        }

        // 禁止在未加密连接上认证，避免密码明文传输
        if (SecurityOf(sender.Security) == SecureSocketOptions.None && !string.IsNullOrEmpty(sender.Username))
        {
            return SendResult.Permanent($"发件账号 {message.SenderProfile} 配置有误：未加密连接禁止配置认证凭据");
        }

        MimeMessage mime;
        try
        {
            mime = Build(message, sender);
        }
        catch (ParseException)
        {
            return SendResult.Permanent("邮件地址或附件的内容类型格式有误");
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
            // 4xx 为暂时性错误（如邮箱繁忙，可稍后重试），5xx 为永久性错误（如邮箱不存在或被拒收）
            return (int)exception.StatusCode >= 500 ? SendResult.Permanent($"邮件服务器拒绝发送：{(int)exception.StatusCode}") : SendResult.Transient($"邮件服务器暂时拒绝发送：{(int)exception.StatusCode}");
        }
        catch (Exception exception) when (exception is AuthenticationException or SaslException or ProtocolException or ServiceNotConnectedException or ServiceNotAuthenticatedException or IOException or SocketException or TimeoutException or OperationCanceledException)
        {
            // 网络、协议、超时及认证异常可在网络或配置恢复后重试
            return SendResult.Transient($"连接或认证失败：{exception.GetType().Name}");
        }
    }

    private static MimeMessage Build(EmailMessage message, EmailSenderOptions sender)
    {
        // Message-ID 固定由投递 ID 生成，保证重试时不变
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

        var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
        foreach (var attachment in message.Attachments)
        {
            builder.Attachments.Add(attachment.Name, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        mime.Body = builder.ToMessageBody();
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
