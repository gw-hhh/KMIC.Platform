namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 请求的访问上下文加载结果
/// </summary>
public enum AccessLoadStatus
{
    /// <summary>
    /// 加载成功
    /// </summary>
    Loaded,

    /// <summary>
    /// 会话无效、已过期，或者用户已禁用、已锁定：按没有登录处理
    /// </summary>
    SessionInvalid,

    /// <summary>
    /// 当前角色已被收回或停用：用户还在登录，但这个角色不能再用了
    /// </summary>
    CurrentRoleUnavailable,
}
