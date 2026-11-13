namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 发短信的渠道
/// </summary>
/// <remarks>
/// 短信服务只接受已审核的模板编码和参数，不能发自由文本。只在投递工作器里调用，在数据库事务之外
/// </remarks>
public interface ISmsGateway
{
    /// <summary>
    /// 发一条短信
    /// </summary>
    /// <param name="message">短信</param>
    /// <param name="ct">取消令牌</param>
    Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct);
}
