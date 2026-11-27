using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>查询各模块声明的权限点。</summary>
[RequirePermission(IdentityPermissions.RoleView)]
public sealed record ListPermissionsQuery : IQuery<IReadOnlyList<PermissionItem>>;
