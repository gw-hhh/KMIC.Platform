namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 软删除
/// </summary>
/// <remarks>
/// 只能加在聚合根上，子实体随聚合根一起标记删除
/// </remarks>
public interface ISoftDeletable;
