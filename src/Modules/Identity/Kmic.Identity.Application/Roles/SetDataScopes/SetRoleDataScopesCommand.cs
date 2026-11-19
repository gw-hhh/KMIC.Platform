using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.SetDataScopes;

/// <summary>
/// 整体替换角色的数据范围：默认等级和各范围对象单独设置的等级
/// </summary>
/// <param name="Id">角色 ID</param>
/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="DefaultLevel">默认等级</param>
/// <param name="Overrides">按范围对象单独设置的等级</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record SetRoleDataScopesCommand(RoleId Id, long ExpectedVersion, RoleScopeLevel DefaultLevel, IReadOnlyDictionary<string, RoleScopeLevel> Overrides) : ICommand<VersionedResponse<RoleId>>;
