using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 调整同级次序的请求
/// </summary>
/// <param name="ExpectedVersion">组织树的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="OrderedChildren">按新顺序排列的全部直属下级 ID（含停用单元），不得重复</param>
public sealed record ReorderOrgUnitsRequest(long ExpectedVersion, IReadOnlyList<OrgUnitId> OrderedChildren);
