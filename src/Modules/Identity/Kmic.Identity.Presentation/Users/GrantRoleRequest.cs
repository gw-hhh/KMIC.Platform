using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Users;

/// <summary>
/// 授予角色的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="RoleId">角色，必须启用</param>
/// <param name="Anchors">锚点：这个角色的“本组织”从哪些组织单元算起，必须是启用的组织单元；不给就从用户所属的组织单元算</param>
public sealed record GrantRoleRequest(long ExpectedVersion, RoleId RoleId, IReadOnlyList<OrgUnitId>? Anchors);
