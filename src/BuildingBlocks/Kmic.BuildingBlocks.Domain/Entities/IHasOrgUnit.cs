using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>标识数据归属特定组织单元；未同时实现 <see cref="IHasOwner" /> 时，“仅本人”数据权限范围不可访问此类数据。</summary>
public interface IHasOrgUnit
{
    /// <summary>所属组织单元 ID。</summary>
    OrgUnitId OrgUnitId { get; }
}
