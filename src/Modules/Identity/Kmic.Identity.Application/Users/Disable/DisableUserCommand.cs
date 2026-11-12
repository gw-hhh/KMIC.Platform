using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Users.Disable;

/// <summary>
/// 禁用用户：他的全部会话随之作废
/// </summary>
/// <remarks>
/// 不能禁用自己，也不能禁用最后一个启用的受保护角色持有人
/// </remarks>
/// <param name="Id">用户</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
/// <param name="Reason">原因，记入状态历史</param>
[RequirePermission(IdentityPermissions.UserManage)]
[Audited]
[Idempotent]
public sealed record DisableUserCommand(UserId Id, long ExpectedVersion, string? Reason) : ICommand<VersionedResponse<UserId>>;
