using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.List;

/// <summary>
/// 用户列表中的角色摘要
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="Name">名称</param>
/// <param name="IsActive">角色是否启用，已停用角色不可用于登录或切换</param>
public sealed record UserRoleBrief(RoleId Id, string Name, bool IsActive);
