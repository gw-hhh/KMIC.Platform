namespace Kmic.BuildingBlocks.Domain.Events;

/// <summary>
/// 领域事件的标记：模块内部、同一事务中处理，不出模块
/// </summary>
/// <remarks>
/// 实现为过去时命名的 sealed record，只携带 ID 与必要的值，不携带实体对象
/// </remarks>
public interface IDomainEvent;
