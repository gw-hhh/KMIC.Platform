using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.SetPermissions;

/// <summary>
/// 整体替换角色的权限点
/// </summary>
/// <remarks>
/// 只接受权限点目录里存在的编码；已经退役的权限点不在目录里，替换时随之去掉。内置的系统管理员角色拥有全部权限点，不能手工改
/// </remarks>
/// <param name="Id">角色</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
/// <param name="Permissions">新的权限点集合</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record SetRolePermissionsCommand(RoleId Id, long ExpectedVersion, IReadOnlyList<string> Permissions) : ICommand<VersionedResponse<RoleId>>;
