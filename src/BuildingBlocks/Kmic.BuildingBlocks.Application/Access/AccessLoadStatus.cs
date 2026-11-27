namespace Kmic.BuildingBlocks.Application.Access;

public enum AccessLoadStatus
{
    Loaded,

    /// <summary>会话无效、已过期，或用户已被禁用、锁定。</summary>
    SessionInvalid,

    /// <summary>当前角色已被收回或停用。</summary>
    CurrentRoleUnavailable,
}
