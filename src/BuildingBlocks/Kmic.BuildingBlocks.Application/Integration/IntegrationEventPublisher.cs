using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 本次命令发布的消息
/// </summary>
/// <remarks>
/// 命令事务保存时把它们写进 Outbox，命令失败时丢弃
/// </remarks>
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
            throw new InvalidOperationException("集成消息只能在命令里发布，要和业务数据一起提交。");
        }

        // 先查目录：没声明的消息在发布时就报错，不要等到保存
        _pending.Add(new PendingMessage(message, catalog.Describe(message.GetType())));
    }
}
