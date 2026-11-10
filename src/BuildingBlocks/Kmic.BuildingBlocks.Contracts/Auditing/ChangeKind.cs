namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 实体的变更方式
/// </summary>
public enum ChangeKind
{
    /// <summary>
    /// 新增
    /// </summary>
    Added,

    /// <summary>
    /// 修改
    /// </summary>
    Modified,

    /// <summary>
    /// 物理删除
    /// </summary>
    Deleted,

    /// <summary>
    /// 软删除（标记删除，可恢复）
    /// </summary>
    SoftDeleted,

    /// <summary>
    /// 从软删除中恢复
    /// </summary>
    Restored,
}
