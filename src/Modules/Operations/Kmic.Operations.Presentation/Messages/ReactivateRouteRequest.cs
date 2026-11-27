namespace Kmic.Operations.Presentation.Messages;

/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
public sealed record ReactivateRouteRequest(string MessageType, string Subscription);
