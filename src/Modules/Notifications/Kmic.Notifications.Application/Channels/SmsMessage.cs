namespace Kmic.Notifications.Application.Channels;

/// <summary>
/// 要发出去的一条短信
/// </summary>
/// <param name="DeliveryId">投递 ID，作为外部流水号</param>
/// <param name="TemplateKey">通知模板键，网关据此找到已审核的短信模板编码与签名</param>
/// <param name="Phone">手机号</param>
/// <param name="Parameters">短信模板的参数</param>
public sealed record SmsMessage(Guid DeliveryId, string TemplateKey, string Phone, IReadOnlyDictionary<string, string> Parameters);
