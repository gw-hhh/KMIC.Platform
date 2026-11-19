using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ResetPassword;

/// <summary>
/// 重置密码并返回临时密码
/// </summary>
/// <remarks>
/// 重复提交会生成新密码
/// </remarks>
[RequirePermission(IdentityPermissions.UserResetPassword)]
[Audited]
[NotIdempotent("响应包含临时密码，不写入幂等记录；每次执行都会生成新密码")]
public sealed record ResetUserPasswordCommand(UserId Id) : ICommand<ResetUserPasswordResponse>;
