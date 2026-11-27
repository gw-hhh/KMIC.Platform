using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Create;

/// <summary>创建不含权限点的角色。</summary>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">显示次序，登录角色下拉列表中的排序</param>
/// <param name="DefaultScopeLevel">默认数据范围等级</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record CreateRoleCommand(string Name, string? Description, int SortOrder, RoleScopeLevel DefaultScopeLevel) : ICommand<CreatedResponse<RoleId>>;
