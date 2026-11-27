namespace Kmic.BuildingBlocks.Domain.Events;

/// <summary>领域事件标记接口，实现类须为过去时命名的 sealed record，且禁止携带实体对象。</summary>
public interface IDomainEvent;
