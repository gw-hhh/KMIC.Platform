using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Presentation.Organization;

/// <summary>
/// 设置用户所属组织单元的请求
/// </summary>
/// <param name="ExpectedVersion">用户的预期版本号，不匹配时返回 409 Common.VersionConflict</param>
/// <param name="OrgUnitId">组织单元，须启用；未指定时清空所属组织</param>
public sealed record AssignUserOrgUnitRequest(long ExpectedVersion, OrgUnitId? OrgUnitId);
