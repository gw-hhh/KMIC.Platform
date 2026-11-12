using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authentication.SwitchRole;

/// <summary>
/// 切换当前角色：同一会话里换一个角色，返回带新角色的访问令牌
/// </summary>
/// <remarks>
/// 旧访问令牌里的角色与会话不一致，下一个请求就会被拒绝，所以切换后旧角色的令牌不能继续用。
/// 当前角色被收回或停用时也要能切换，所以接口层要放行“当前角色不可用”的状态
/// </remarks>
/// <param name="RoleId">要切换到的角色</param>
[AllowAuthenticated]
[NotAudited("结果记入安全事件")]
[NotIdempotent("每次切换签发新的访问令牌，重试没有副作用")]
public sealed record SwitchRoleCommand(RoleId RoleId) : ICommand<AuthTokens>;
