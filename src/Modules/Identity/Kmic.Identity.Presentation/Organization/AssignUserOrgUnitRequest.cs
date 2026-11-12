using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 设置用户所属组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的用户版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="OrgUnitId">组织单元，必须启用；不给表示不属于任何单元</param>
public sealed record AssignUserOrgUnitRequest(long ExpectedVersion, OrgUnitId? OrgUnitId);
