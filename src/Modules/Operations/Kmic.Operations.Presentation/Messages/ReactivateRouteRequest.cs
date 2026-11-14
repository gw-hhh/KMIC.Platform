namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 重新激活订阅的请求
/// </summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
public sealed record ReactivateRouteRequest(string MessageType, string Subscription);
