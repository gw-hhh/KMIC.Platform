using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Grants.RevokeRole;

/// <summary>
/// 收回用户的一个角色
/// </summary>
/// <remarks>
/// 不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能收回；
/// 不能收回最后一个启用的受保护角色持有人的受保护角色。正在用这个角色的会话，下一个请求起得到 403，要重新选择角色
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回版本冲突</param>
/// <param name="RoleId">角色</param>
[RequirePermission(IdentityPermissions.UserGrant)]
[Audited]
[Idempotent]
public sealed record RevokeRoleCommand(UserId Id, long ExpectedVersion, RoleId RoleId) : ICommand<VersionedResponse<UserId>>;
