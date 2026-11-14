using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.ResetPassword;

/// <summary>
/// 重置用户密码并返回临时密码，作废全部会话，下次登录须修改密码
/// </summary>
/// <remarks>
/// 临时密码仅在本次响应返回，不写入幂等记录；重复提交会生成新密码，旧临时密码随即失效。
/// 目标用户持有受保护角色时，操作者的当前角色也必须受保护
/// </remarks>
[RequirePermission(IdentityPermissions.UserResetPassword)]
[Audited]
[NotIdempotent("响应包含临时密码，不写入幂等记录；每次执行都会生成新密码")]
public sealed record ResetUserPasswordCommand(UserId Id) : ICommand<ResetUserPasswordResponse>;
