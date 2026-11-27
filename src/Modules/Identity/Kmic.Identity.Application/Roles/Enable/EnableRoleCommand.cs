using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Roles.Enable;

/// <summary>启用已停用的角色。</summary>
/// <param name="Id">角色 ID</param>
/// <param name="ExpectedVersion">预期版本号</param>
[RequirePermission(IdentityPermissions.RoleManage)]
[Audited]
[Idempotent]
public sealed record EnableRoleCommand(RoleId Id, long ExpectedVersion) : ICommand<VersionedResponse<RoleId>>;
