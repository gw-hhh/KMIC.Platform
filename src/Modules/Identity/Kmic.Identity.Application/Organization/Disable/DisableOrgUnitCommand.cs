using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Disable;

/// <summary>
/// 停用组织单元
/// </summary>
/// <remarks>
/// 还有启用的下级、或有启用的用户归属于它时拒绝；根单元不能停用。停用的单元仍然在树上，历史数据还引用着它
/// </remarks>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回版本冲突</param>
/// <param name="Id">组织单元</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record DisableOrgUnitCommand(long ExpectedVersion, OrgUnitId Id) : ICommand<VersionedResponse<OrgTreeId>>;
