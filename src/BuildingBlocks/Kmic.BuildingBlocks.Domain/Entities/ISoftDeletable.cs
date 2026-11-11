namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 聚合根删除时只做标记，可恢复；子实体随聚合根一起被标记，从聚合中移除的子实体仍是物理删除
/// </summary>
/// <remarks>
/// 只有聚合根能实现；删除标记列、过滤器与部分唯一索引由基础设施按约定添加
/// </remarks>
public interface ISoftDeletable;
