using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一个订阅对一种消息版本的处理
/// </summary>
/// <param name="Name">订阅名</param>
/// <param name="Module">订阅方模块</param>
/// <param name="Message">订阅的消息</param>
/// <param name="CommandType">消息转成的命令</param>
/// <param name="Map">把消息转成命令</param>
/// <param name="IsOrdered">是否按顺序键依次处理</param>
/// <param name="Rebuild">重建读模型的方式，没有时为空</param>
internal sealed record SubscriptionDefinition(
    string Name,
    string Module,
    IntegrationMessageType Message,
    Type CommandType,
    Func<object, MessageEnvelope, ICommand> Map,
    bool IsOrdered,
    IReadModelRebuild? Rebuild);
