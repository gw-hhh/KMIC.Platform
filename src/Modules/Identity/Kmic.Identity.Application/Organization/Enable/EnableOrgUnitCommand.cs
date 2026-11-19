using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Enable;

/// <summary>
/// 启用组织单元；它的上级必须是启用的
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="Id">组织单元 ID</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record EnableOrgUnitCommand(long ExpectedVersion, OrgUnitId Id) : ICommand<VersionedResponse<OrgTreeId>>;
