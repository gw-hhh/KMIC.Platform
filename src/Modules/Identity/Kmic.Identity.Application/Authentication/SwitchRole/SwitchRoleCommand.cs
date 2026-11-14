using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authentication.SwitchRole;

/// <summary>
/// 切换当前会话的角色
/// </summary>
[AllowAuthenticated]
[NotAudited("通过 RoleSwitched 安全事件记录角色切换")]
[NotIdempotent("切换结果含访问令牌，不写入幂等记录")]
public sealed record SwitchRoleCommand(RoleId RoleId) : ICommand<AuthTokens>;
