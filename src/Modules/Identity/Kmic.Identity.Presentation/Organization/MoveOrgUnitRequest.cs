using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 移动组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的组织树版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="NewParentId">新的上级，必须启用，不能是自己或自己的下级</param>
public sealed record MoveOrgUnitRequest(long ExpectedVersion, OrgUnitId NewParentId);
