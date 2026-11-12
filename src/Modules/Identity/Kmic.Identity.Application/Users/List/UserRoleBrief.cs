using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.List;

/// <summary>
/// 用户列表里一个角色的简要信息
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="Name">名称</param>
/// <param name="IsActive">角色是否启用；停用的角色用户登录后用不了</param>
public sealed record UserRoleBrief(RoleId Id, string Name, bool IsActive);
