using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.Authorization.Explain;

/// <summary>
/// 组织单元
/// </summary>
/// <param name="Id">组织单元 ID</param>
/// <param name="Name">名称，组织单元已删除时为空</param>
public sealed record ExplainedOrgUnit(OrgUnitId Id, string? Name);
