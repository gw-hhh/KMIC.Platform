using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 设置用户所属组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">用户的预期版本号</param>
/// <param name="OrgUnitId">组织单元</param>
public sealed record AssignUserOrgUnitRequest(long ExpectedVersion, OrgUnitId? OrgUnitId);
