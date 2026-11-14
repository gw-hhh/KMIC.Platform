namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 短信发送渠道
/// </summary>
/// <remarks>
/// 仅发送已审核的模板编码和参数，不支持自由文本；仅由投递工作器在数据库事务之外调用
/// </remarks>
public interface ISmsGateway
{
    /// <summary>
    /// 发送短信
    /// </summary>
    Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct);
}
