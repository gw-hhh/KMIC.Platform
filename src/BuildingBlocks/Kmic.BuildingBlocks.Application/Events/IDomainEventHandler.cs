using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 领域事件处理器
/// </summary>
/// <remarks>
/// 和命令在同一个事务里执行，返回失败会让整个命令失败。只能改本模块的数据，多个处理器之间不保证顺序
/// </remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "处理的是领域事件，不是 .NET 的 event 委托")]
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// 处理事件
    /// </summary>
    ValueTask<Result> HandleAsync(TEvent domainEvent, CancellationToken ct);
}
