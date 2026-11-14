namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 实例声明的一个订阅
/// </summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Module">订阅方模块</param>
public sealed record InstanceSubscription(string MessageType, string Subscription, string Module);
