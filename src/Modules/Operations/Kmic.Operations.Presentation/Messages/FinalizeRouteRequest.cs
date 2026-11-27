namespace Kmic.Operations.Presentation.Messages;

/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Reason">终结原因</param>
/// <param name="AbandonUnfinished">是否放弃未完成的消息</param>
public sealed record FinalizeRouteRequest(string MessageType, string Subscription, string Reason, bool AbandonUnfinished = false);
