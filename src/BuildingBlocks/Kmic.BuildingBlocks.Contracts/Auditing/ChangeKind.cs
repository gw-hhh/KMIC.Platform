namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 变更类型
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
    /// 软删除
    /// </summary>
    SoftDeleted,

    /// <summary>
    /// 恢复
    /// </summary>
    Restored,
}
