using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.RevokeRole;

/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">用户的预期版本号</param>
/// <param name="RoleId">角色 ID</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record RevokeRoleCommand(UserId Id, long ExpectedVersion, RoleId RoleId) : ICommand<VersionedResponse<UserId>>;
