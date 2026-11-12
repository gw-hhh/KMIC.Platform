using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.GrantRole;

/// <summary>
/// 授予用户一个角色
/// </summary>
/// <remarks>
/// 角色必须启用，锚点必须是启用的组织单元。不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能授予
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回版本冲突</param>
/// <param name="RoleId">角色</param>
/// <param name="Anchors">锚点：这个角色的“本组织”从哪些组织单元算起；不给就从用户所属的组织单元算</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record GrantRoleCommand(UserId Id, long ExpectedVersion, RoleId RoleId, IReadOnlyList<OrgUnitId> Anchors) : ICommand<VersionedResponse<UserId>>;
