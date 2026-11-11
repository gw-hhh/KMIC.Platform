namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 拒绝原因
/// </summary>
public enum DenialReason
{
    /// <summary>
    /// 没有权限点
    /// </summary>
    MissingPermission,

    /// <summary>
    /// 不在数据权限范围内
    /// </summary>
    OutOfDataScope,

    /// <summary>
    /// 资源策略不允许
    /// </summary>
    ResourceDenied,

    /// <summary>
    /// 不是被指派人
    /// </summary>
    AssignmentMismatch,
}
