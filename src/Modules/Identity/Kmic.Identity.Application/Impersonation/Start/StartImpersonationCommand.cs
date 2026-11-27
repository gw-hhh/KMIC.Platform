using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authentication;

namespace Kmic.Identity.Application.Impersonation.Start;

/// <summary>以目标用户身份创建模拟会话以排查访问差异（是否允许写入创建时确定，不可变更）</summary>
/// <param name="TargetUserId">被模拟的用户</param>
/// <param name="RoleId">目标用户的角色 ID</param>
/// <param name="Reason">原因</param>
/// <param name="ClientIp">操作者 IP</param>
/// <param name="UserAgent">操作者标识（User-Agent）</param>
[RequirePermission(IdentityPermissions.UserImpersonate)]
[Audited]
[NotIdempotent("响应包含访问令牌，不可写入幂等记录；重复提交将开启新的模拟会话")]
public sealed record StartImpersonationCommand(UserId TargetUserId, RoleId? RoleId, string Reason, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>;
