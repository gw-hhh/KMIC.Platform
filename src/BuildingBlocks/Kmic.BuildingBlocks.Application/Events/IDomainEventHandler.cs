using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 领域事件处理器：与命令在同一事务中执行，返回失败会让整个命令失败（05-01 D7）
/// </summary>
/// <remarks>
/// 同一事件的多个处理器不保证顺序；只能修改本模块数据，对外通知经 IIntegrationEventPublisher
/// </remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "05-02 第 6.2 节定的名称，它处理的是领域事件，不是 .NET 的 event 委托")]
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// 处理事件
    /// </summary>
    ValueTask<Result> HandleAsync(TEvent domainEvent, CancellationToken ct);
}
