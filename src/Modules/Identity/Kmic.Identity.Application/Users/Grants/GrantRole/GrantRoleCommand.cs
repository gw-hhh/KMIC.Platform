using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.GrantRole;

/// <summary>
/// 授予用户一个角色
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回版本冲突</param>
/// <param name="RoleId">角色 ID</param>
/// <param name="Anchors">锚点：这个角色的“本组织”从哪些组织单元算起；不给就从用户所属的组织单元算</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record GrantRoleCommand(UserId Id, long ExpectedVersion, RoleId RoleId, IReadOnlyList<OrgUnitId> Anchors) : ICommand<VersionedResponse<UserId>>;
