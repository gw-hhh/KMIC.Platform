using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Reorder;

/// <summary>
/// 调整某个单元下全部直接下级的先后次序
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回版本冲突</param>
/// <param name="ParentId">上级组织单元</param>
/// <param name="OrderedChildren">按新次序排好的全部直接下级，不能多也不能少</param>
[RequirePermission(IdentityPermissions.OrgUnitManage)]
[Audited]
[Idempotent]
public sealed record ReorderOrgUnitsCommand(long ExpectedVersion, OrgUnitId ParentId, IReadOnlyList<OrgUnitId> OrderedChildren) : ICommand<VersionedResponse<OrgTreeId>>;
