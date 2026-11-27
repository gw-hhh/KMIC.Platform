using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Update;

/// <summary>修改组织单元的名称和编码</summary>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="Id">组织单元 ID</param>
/// <param name="Name">名称</param>
/// <param name="Code">编码</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record UpdateOrgUnitCommand(long ExpectedVersion, OrgUnitId Id, string Name, string? Code) : ICommand<VersionedResponse<OrgTreeId>>;
