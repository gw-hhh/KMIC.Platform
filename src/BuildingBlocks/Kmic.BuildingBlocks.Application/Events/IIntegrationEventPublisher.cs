using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 在命令内登记要发出的集成事件与请求型消息，保存时随业务数据写入本模块 Outbox
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// 发布集成事件
    /// </summary>
    void Add(IIntegrationEvent integrationEvent);

    /// <summary>
    /// 发送请求型消息（只能发给定义它的平台模块）
    /// </summary>
    void Send(IIntegrationRequest request);
}
