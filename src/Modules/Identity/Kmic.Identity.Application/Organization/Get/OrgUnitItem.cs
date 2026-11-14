using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Domain.Organization;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 组织单元列表项
/// </summary>
/// <param name="Id">组织单元 ID</param>
/// <param name="ParentId">上级组织单元 ID，根单元为空</param>
/// <param name="Name">名称</param>
/// <param name="Code">编码</param>
/// <param name="Status">状态</param>
/// <param name="SortOrder">在同一上级下的次序</param>
public sealed record OrgUnitItem(OrgUnitId Id, OrgUnitId? ParentId, string Name, string? Code, OrgUnitStatus Status, int SortOrder);
