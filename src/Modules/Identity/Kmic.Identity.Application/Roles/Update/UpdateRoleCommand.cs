using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.Update;

/// <summary>修改角色的名称、说明和次序。</summary>
/// <param name="Id">角色 ID</param>
/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="Name">名称</param>
/// <param name="Description">说明</param>
/// <param name="SortOrder">次序</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record UpdateRoleCommand(RoleId Id, long ExpectedVersion, string Name, string? Description, int SortOrder) : ICommand<VersionedResponse<RoleId>>;
