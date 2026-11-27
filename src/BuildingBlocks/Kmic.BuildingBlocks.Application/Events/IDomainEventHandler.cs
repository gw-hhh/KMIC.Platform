using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>领域事件处理器；与所属命令共享同一数据库事务，任一处理器返回失败将导致整个命令回滚。</summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "处理领域事件而非 .NET event 委托")]
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    ValueTask<Result> HandleAsync(TEvent domainEvent, CancellationToken ct);
}
