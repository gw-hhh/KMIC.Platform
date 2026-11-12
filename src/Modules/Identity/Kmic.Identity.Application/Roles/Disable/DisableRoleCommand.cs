using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.Disable;

/// <summary>
/// 停用角色
/// </summary>
/// <remarks>
/// 有持有人的角色也可以停用：他们正在用它的会话，下一个请求起得到 403，要重新选择角色。界面在停用前提示受影响的人数。
/// 内置的系统管理员角色不能停用
/// </remarks>
/// <param name="Id">角色</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
/// <param name="Reason">原因，记入状态历史</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record DisableRoleCommand(RoleId Id, long ExpectedVersion, string? Reason) : ICommand<VersionedResponse<RoleId>>;
