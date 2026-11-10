namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 集成事件：发布方告诉其他模块“发生了什么”，可以有任意多个订阅方
/// </summary>
/// <remarks>
/// 载荷包含订阅方处理所需的数据，订阅方不为了处理事件再回调发布方
/// </remarks>
public interface IIntegrationEvent : IIntegrationMessage;
