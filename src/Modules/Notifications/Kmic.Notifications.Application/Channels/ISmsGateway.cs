namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 短信发送渠道
/// </summary>
/// <remarks>
/// 只发已审核的模板，不支持自由文本
/// </remarks>
public interface ISmsGateway
{
    /// <summary>
    /// 发送短信
    /// </summary>
    Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct);
}
