namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 邮件发送渠道
/// </summary>
/// <remarks>
/// 要区分暂时失败和永久失败
/// </remarks>
public interface IEmailSender
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct);
}
