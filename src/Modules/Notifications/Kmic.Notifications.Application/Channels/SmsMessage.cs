namespace Kmic.Notifications.Application.Channels;

/// <summary>待发送短信</summary>
/// <param name="DeliveryId">投递 ID</param>
/// <param name="TemplateKey">通知模板键</param>
/// <param name="Phone">手机号</param>
/// <param name="Parameters">短信模板参数</param>
public sealed record SmsMessage(Guid DeliveryId, string TemplateKey, string Phone, IReadOnlyDictionary<string, string> Parameters);
