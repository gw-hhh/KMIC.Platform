using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Update;

/// <summary>
/// 修改组织单元的名称和编码
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回版本冲突</param>
/// <param name="Id">组织单元</param>
/// <param name="Name">名称，同一上级下不区分大小写唯一</param>
/// <param name="Code">编码，可以不填</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record UpdateOrgUnitCommand(long ExpectedVersion, OrgUnitId Id, string Name, string? Code) : ICommand<VersionedResponse<OrgTreeId>>;
