using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Disable;

/// <summary>
/// 禁用用户并作废其全部会话
/// </summary>
/// <remarks>
/// 预期版本不匹配时返回版本冲突；原因记入状态历史
/// </remarks>
[RequirePermission(IdentityPermissions.UserManage)]
[Audited]
[Idempotent]
public sealed record DisableUserCommand(UserId Id, long ExpectedVersion, string? Reason) : ICommand<VersionedResponse<UserId>>;
