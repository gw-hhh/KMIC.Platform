namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 发邮件的渠道
/// </summary>
/// <remarks>
/// 只在投递工作器里调用，在数据库事务之外。失败要分清暂时的和永久的：暂时的退避重试，永久的不再重试
/// </remarks>
public interface IEmailSender
{
    /// <summary>
    /// 发一封邮件
    /// </summary>
    /// <param name="message">邮件</param>
    /// <param name="ct">取消令牌</param>
    Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct);
}
