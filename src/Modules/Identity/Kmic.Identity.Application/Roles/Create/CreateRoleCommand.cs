using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Create;

/// <summary>
/// 新建角色：没有任何权限点，之后用设置权限点与数据范围的命令配置
/// </summary>
/// <param name="Name">名称，不区分大小写唯一</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">登录时角色下拉里的次序</param>
/// <param name="DefaultScopeLevel">默认数据范围等级</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record CreateRoleCommand(string Name, string? Description, int SortOrder, RoleScopeLevel DefaultScopeLevel) : ICommand<CreatedResponse<RoleId>>;
