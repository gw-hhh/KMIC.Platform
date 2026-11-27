using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

// 模块声明的原始订阅记录（尚未校验）
internal sealed record DeclaredSubscription(string Name, Type MessageType, Type CommandType, Func<object, MessageEnvelope, ICommand> Map, SubscriptionOptions Options);
