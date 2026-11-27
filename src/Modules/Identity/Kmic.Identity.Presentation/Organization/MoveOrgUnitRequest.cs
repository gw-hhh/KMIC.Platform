using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <param name="ExpectedVersion">组织树的预期版本号</param>
/// <param name="NewParentId">新的上级</param>
public sealed record MoveOrgUnitRequest(long ExpectedVersion, OrgUnitId NewParentId);
