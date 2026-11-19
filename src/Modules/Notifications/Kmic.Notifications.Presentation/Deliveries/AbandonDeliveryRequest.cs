namespace Kmic.Notifications.Presentation.Deliveries;

/// <summary>
/// 放弃一条失败投递的请求
/// </summary>
/// <param name="Reason">放弃的原因</param>
public sealed record AbandonDeliveryRequest(string Reason);
