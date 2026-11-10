namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 授权拒绝的原因，写入安全事件，供日志、测试与前端映射使用
/// </summary>
public enum DenialReason
{
    /// <summary>
    /// 当前角色没有权限点（第 1 层）
    /// </summary>
    MissingPermission,

    /// <summary>
    /// 数据在可见范围之外（第 2 层）
    /// </summary>
    OutOfDataScope,

    /// <summary>
    /// 资源策略不允许（第 3 层）
    /// </summary>
    ResourceDenied,

    /// <summary>
    /// 不是这条数据的被指派人（第 3 层）
    /// </summary>
    AssignmentMismatch,
}
