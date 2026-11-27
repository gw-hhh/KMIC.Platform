namespace Kmic.Notifications.Application.Channels;

/// <summary>短信发送渠道（仅支持已审核模板，不支持自由文本）</summary>
public interface ISmsGateway
{
    Task<SendResult> SendAsync(SmsMessage message, CancellationToken ct);
}
