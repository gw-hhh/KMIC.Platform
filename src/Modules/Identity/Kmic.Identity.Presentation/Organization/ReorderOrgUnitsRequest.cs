using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="OrderedChildren">重排后的下级 ID，须包含全部直属下级（含停用单元）</param>
public sealed record ReorderOrgUnitsRequest(long ExpectedVersion, IReadOnlyList<OrgUnitId> OrderedChildren);
