namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 终结订阅的请求
/// </summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Reason">原因，必填</param>
/// <param name="AbandonUnfinished">是否放弃未完成的消息，默认为 false；未选择放弃且有未完成消息时拒绝终结</param>
public sealed record FinalizeRouteRequest(string MessageType, string Subscription, string Reason, bool AbandonUnfinished = false);
