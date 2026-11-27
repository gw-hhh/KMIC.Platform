namespace Kmic.Notifications.Application.Channels;

/// <summary>邮件发送渠道（实现须区分暂时失败与永久失败）</summary>
public interface IEmailSender
{
    Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct);
}
