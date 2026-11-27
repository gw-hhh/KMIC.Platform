namespace Kmic.Notifications.Presentation.Deliveries;

/// <param name="Reason">放弃原因</param>
public sealed record AbandonDeliveryRequest(string Reason);
