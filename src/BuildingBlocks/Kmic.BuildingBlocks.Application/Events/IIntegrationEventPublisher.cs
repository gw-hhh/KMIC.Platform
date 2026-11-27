using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>集成消息发布器，仅限在命令处理器或领域事件处理器中调用。</summary>
public interface IIntegrationEventPublisher
{
    void Add(IIntegrationEvent integrationEvent);

    void Send(IIntegrationRequest request);
}
