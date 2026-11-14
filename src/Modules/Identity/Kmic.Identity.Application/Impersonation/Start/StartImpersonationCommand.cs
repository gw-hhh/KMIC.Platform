using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Application.Authentication;

namespace Kmic.Identity.Application.Impersonation.Start;

/// <summary>
/// 以目标用户身份创建模拟会话，用于排查访问差异
/// </summary>
/// <remarks>
/// 是否允许写入在创建会话时确定，会话期间不变
/// </remarks>
/// <param name="TargetUserId">被模拟的用户，必须启用</param>
/// <param name="RoleId">目标用户的角色 ID；未指定时优先选择上次角色，不可用时选择排序最前的启用角色</param>
/// <param name="Reason">原因，必填，写入安全事件</param>
/// <param name="ClientIp">操作者的 IP，由接口层填，不是请求体的一部分</param>
/// <param name="UserAgent">操作者的 User-Agent，由接口层填，不是请求体的一部分</param>
[RequirePermission(IdentityPermissions.UserImpersonate)]
[Audited]
[NotIdempotent("响应里有访问令牌，不能存进幂等记录；重复提交会开始另一个模拟会话")]
public sealed record StartImpersonationCommand(UserId TargetUserId, RoleId? RoleId, string Reason, string? ClientIp, string? UserAgent) : ICommand<AuthTokens>;
