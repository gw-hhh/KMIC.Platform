namespace Kmic.BuildingBlocks.Domain.Events;

/// <summary>
/// 领域事件
/// </summary>
/// <remarks>
/// 用过去时命名的 sealed record，只带 ID 和必要的值，不带实体对象
/// </remarks>
public interface IDomainEvent;
