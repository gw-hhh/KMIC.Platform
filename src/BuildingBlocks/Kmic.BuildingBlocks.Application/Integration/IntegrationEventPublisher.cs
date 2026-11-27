using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

// 当前命令执行期间待发布的集成消息收集器
internal sealed class IntegrationEventPublisher(IExecutionContext execution, IntegrationMessageCatalog catalog) : IIntegrationEventPublisher
{
    private readonly List<PendingMessage> _pending = [];

    public void Add(IIntegrationEvent integrationEvent) => Enqueue(integrationEvent);

    public void Send(IIntegrationRequest request) => Enqueue(request);

    public IReadOnlyList<PendingMessage> TakePending()
    {
        var pending = _pending.ToArray();
        _pending.Clear();
        return pending;
    }

    public void Discard() => _pending.Clear();

    private void Enqueue(IIntegrationMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!execution.InCommand)
        {
            throw new InvalidOperationException("集成消息仅限在命令执行期间发布，以保证与业务数据同事务提交。");
        }

        // 提前校验目录：未声明的消息在发布阶段即报错，避免延迟至提交时失败
        _pending.Add(new PendingMessage(message, catalog.Describe(message.GetType())));
    }
}
