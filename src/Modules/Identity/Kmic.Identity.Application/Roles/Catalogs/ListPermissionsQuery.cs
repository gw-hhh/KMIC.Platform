using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>
/// 查询权限点目录：全部模块声明的权限点，按模块与分组排好，角色管理页据此勾选
/// </summary>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record ListPermissionsQuery : IQuery<IReadOnlyList<PermissionItem>>;
