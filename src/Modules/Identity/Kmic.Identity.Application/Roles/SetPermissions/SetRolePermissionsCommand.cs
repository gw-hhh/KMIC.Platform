using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.SetPermissions;

/// <summary>
/// 整体替换角色的权限点
/// </summary>
/// <remarks>
/// 提交未声明或已退役的编码时拒绝；替换后只保留本次提交的权限点。
/// 内置系统管理员角色拥有全部权限点，不能手动修改
/// </remarks>
/// <param name="Id">角色 ID</param>
/// <param name="ExpectedVersion">预期版本号，不匹配时返回版本冲突</param>
/// <param name="Permissions">新的权限点集合</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record SetRolePermissionsCommand(RoleId Id, long ExpectedVersion, IReadOnlyList<string> Permissions) : ICommand<VersionedResponse<RoleId>>;
