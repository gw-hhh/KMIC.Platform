namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 邮件发送渠道
/// </summary>
/// <remarks>
/// 仅由投递工作器在数据库事务之外调用。须区分暂时失败和永久失败：前者退避重试，后者停止自动重试
/// </remarks>
public interface IEmailSender
{
    /// <summary>
    /// 发送邮件
    /// </summary>
    Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct);
}
