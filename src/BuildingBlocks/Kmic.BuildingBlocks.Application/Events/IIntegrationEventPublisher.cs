using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 集成事件发布
/// </summary>
/// <remarks>
/// 只能在命令处理器和领域事件处理器里调用。消息和业务数据在同一个事务里写入 Outbox，命令失败就不会发出
/// </remarks>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// 发布集成事件
    /// </summary>
    void Add(IIntegrationEvent integrationEvent);

    /// <summary>
    /// 发送请求型消息
    /// </summary>
    void Send(IIntegrationRequest request);
}
