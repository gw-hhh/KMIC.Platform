namespace Kmic.Notifications.Presentation.Deliveries;

/// <summary>
/// 放弃一条失败投递的请求
/// </summary>
/// <param name="Reason">放弃的原因，必填，最多 500 个字符</param>
public sealed record AbandonDeliveryRequest(string Reason);
