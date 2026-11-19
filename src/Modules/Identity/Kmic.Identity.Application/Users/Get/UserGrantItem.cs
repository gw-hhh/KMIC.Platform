using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 用户的角色授予信息
/// </summary>
/// <param name="RoleId">角色 ID</param>
/// <param name="RoleName">角色名称</param>
/// <param name="RoleIsActive">角色是否启用</param>
/// <param name="RoleIsProtected">是否受保护角色</param>
/// <param name="GrantedAt">授予时间</param>
/// <param name="GrantedBy">授予人 ID</param>
/// <param name="GrantedByName">授予人的姓名</param>
/// <param name="Anchors">组织锚点</param>
public sealed record UserGrantItem(
    RoleId RoleId,
    string? RoleName,
    bool RoleIsActive,
    bool RoleIsProtected,
    DateTimeOffset GrantedAt,
    UserId GrantedBy,
    string? GrantedByName,
    IReadOnlyList<UserAnchorItem> Anchors);
