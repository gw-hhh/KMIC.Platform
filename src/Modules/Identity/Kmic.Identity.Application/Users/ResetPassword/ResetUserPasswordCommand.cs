using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ResetPassword;

/// <summary>
/// 管理员重置用户的密码：生成一次性临时密码，用户的全部会话作废，下次登录必须先改密码
/// </summary>
/// <remarks>
/// 响应里的临时密码只出现这一次。所以它不做幂等：幂等记录会把响应存进数据库；重复提交就是重新生成一个，
/// 之前的临时密码随之失效。对持有受保护角色的用户，只有同样持有受保护角色的人才能重置
/// </remarks>
/// <param name="Id">用户</param>
[RequirePermission(IdentityPermissions.UserResetPassword)]
[Audited]
[NotIdempotent("响应里有一次性临时密码，不能存进幂等记录；重复提交会重新生成，之前的临时密码随之失效")]
public sealed record ResetUserPasswordCommand(UserId Id) : ICommand<ResetUserPasswordResponse>;
