using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 授予角色的请求
/// </summary>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="RoleId">角色，必须启用</param>
/// <param name="Anchors">组织锚点，须启用；未指定或为空列表时以用户所属组织为起点</param>
public sealed record GrantRoleRequest(long ExpectedVersion, RoleId RoleId, IReadOnlyList<OrgUnitId>? Anchors);
