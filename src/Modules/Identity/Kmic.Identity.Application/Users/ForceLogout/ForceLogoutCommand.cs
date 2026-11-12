using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ForceLogout;

/// <summary>
/// 强制用户下线：指定的一个会话，或他的全部会话（含他发起的模拟会话）
/// </summary>
/// <remarks>
/// 管理员不能对自己用，自己的登录设备在“我的登录设备”里管理。对持有受保护角色的用户，只有同样持有受保护角色的人才能用
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="SessionId">要下线的会话；不给就是全部</param>
[RequirePermission(IdentityPermissions.UserForceLogout)]
[Audited]
[Idempotent]
public sealed record ForceLogoutCommand(UserId Id, SessionId? SessionId) : ICommand<VersionedResponse<UserId>>;
