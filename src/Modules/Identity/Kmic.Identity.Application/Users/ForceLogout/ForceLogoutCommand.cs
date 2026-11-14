using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ForceLogout;

/// <summary>
/// 结束用户的指定会话或全部会话，包括相关的模拟会话
/// </summary>
/// <remarks>
/// 不能对自己执行此操作，请通过“我的登录设备”管理自己的会话。
/// 目标用户持有受保护角色时，操作者的当前角色也必须受保护
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="SessionId">要下线的会话；不给就是全部</param>
[RequirePermission(IdentityPermissions.UserForceLogout)]
[Audited]
[Idempotent]
public sealed record ForceLogoutCommand(UserId Id, SessionId? SessionId) : ICommand<VersionedResponse<UserId>>;
