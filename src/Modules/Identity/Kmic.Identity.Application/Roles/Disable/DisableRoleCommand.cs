using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.Disable;

/// <summary>
/// 停用角色
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回版本冲突</param>
/// <param name="Reason">原因，记入状态历史</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record DisableRoleCommand(RoleId Id, long ExpectedVersion, string? Reason) : ICommand<VersionedResponse<RoleId>>;
