namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 集成事件
/// </summary>
/// <remarks>
/// 事件里要带上订阅方处理所需的数据，订阅方不回头查发布方
/// </remarks>
public interface IIntegrationEvent : IIntegrationMessage;
