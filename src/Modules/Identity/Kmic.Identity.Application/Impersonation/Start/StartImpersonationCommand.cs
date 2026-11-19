using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authentication;

namespace Kmic.Identity.Application.Impersonation.Start;

/// <summary>
/// 以目标用户身份创建模拟会话，用于排查访问差异
/// </summary>
/// <remarks>
/// 能否写入在创建时定下，之后不变
/// </remarks>
/// <param name="TargetUserId">被模拟的用户</param>
/// <param name="RoleId">目标用户的角色 ID</param>
/// <param name="Reason">原因</param>
/// <param name="ClientIp">操作者的 IP</param>
/// <param name="UserAgent">操作者的 User-Agent</param>
[RequirePermission(IdentityPermissions.UserImpersonate)]
[Audited]
[NotIdempotent("响应里有访问令牌，不能存进幂等记录；重复提交会开始另一个模拟会话")]
public sealed record StartImpersonationCommand(UserId TargetUserId, RoleId? RoleId, string Reason, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>;
