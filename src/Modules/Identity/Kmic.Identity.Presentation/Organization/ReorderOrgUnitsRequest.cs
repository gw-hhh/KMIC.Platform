using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 调整同级次序的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="OrderedChildren">按新次序排好的全部直接下级，不能多也不能少，不能重复</param>
public sealed record ReorderOrgUnitsRequest(long ExpectedVersion, IReadOnlyList<OrgUnitId> OrderedChildren);
