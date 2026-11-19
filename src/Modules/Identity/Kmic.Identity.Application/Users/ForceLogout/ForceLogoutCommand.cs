using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ForceLogout;

/// <summary>
/// 结束用户的指定会话或全部会话，包括相关的模拟会话
/// </summary>
/// <remarks>
/// 不能对自己执行，自己的会话在“我的登录设备”里管理；目标持有受保护角色时，操作者当前角色也须受保护
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="SessionId">目标会话 ID；为 null 时处理全部相关会话</param>
[RequirePermission(IdentityPermissions.UserForceLogout)]
[Audited]
[Idempotent]
public sealed record ForceLogoutCommand(UserId Id, SessionId? SessionId) : ICommand<VersionedResponse<UserId>>;
