using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 集成事件发布
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// 发布集成事件
    /// </summary>
    void Add(IIntegrationEvent integrationEvent);

    /// <summary>
    /// 发送请求消息
    /// </summary>
    void Send(IIntegrationRequest request);
}
