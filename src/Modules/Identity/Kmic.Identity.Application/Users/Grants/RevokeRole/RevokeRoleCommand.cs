using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.RevokeRole;

/// <summary>
/// 收回用户的一个角色
/// </summary>
/// <param name="Id">用户 ID</param>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回版本冲突</param>
/// <param name="RoleId">角色 ID</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record RevokeRoleCommand(UserId Id, long ExpectedVersion, RoleId RoleId) : ICommand<VersionedResponse<UserId>>;
