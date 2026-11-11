using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 模块写下的一条订阅，还没校验
/// </summary>
/// <param name="Name">订阅名</param>
/// <param name="MessageType">消息类型</param>
/// <param name="CommandType">命令类型</param>
/// <param name="Map">把消息转成命令</param>
/// <param name="Options">有序处理、重建方式</param>
internal sealed record DeclaredSubscription(string Name, Type MessageType, Type CommandType, Func<object, MessageEnvelope, ICommand> Map, SubscriptionOptions Options);
