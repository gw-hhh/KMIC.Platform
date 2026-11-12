using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Enable;

/// <summary>
/// 启用组织单元；它的上级必须是启用的
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回版本冲突</param>
/// <param name="Id">组织单元</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record EnableOrgUnitCommand(long ExpectedVersion, OrgUnitId Id) : ICommand<VersionedResponse<OrgTreeId>>;
