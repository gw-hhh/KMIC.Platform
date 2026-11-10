namespace Kmic.BuildingBlocks.Contracts.References;

/// <summary>
/// 被引用对象当前的状态，界面据此标注“[已禁用]”“[已删除]”
/// </summary>
public enum ReferenceStatus
{
    /// <summary>
    /// 正常，可以被新数据选用
    /// </summary>
    Active,

    /// <summary>
    /// 已停用：历史引用照常显示，不能被新数据选用
    /// </summary>
    Disabled,

    /// <summary>
    /// 已删除（软删除）：历史引用照常显示，不能被新数据选用
    /// </summary>
    Deleted,
}
