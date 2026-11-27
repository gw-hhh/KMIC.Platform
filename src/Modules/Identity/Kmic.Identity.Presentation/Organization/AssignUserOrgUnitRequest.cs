using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <param name="ExpectedVersion">用户的预期版本号</param>
/// <param name="OrgUnitId">组织单元</param>
public sealed record AssignUserOrgUnitRequest(long ExpectedVersion, OrgUnitId? OrgUnitId);
