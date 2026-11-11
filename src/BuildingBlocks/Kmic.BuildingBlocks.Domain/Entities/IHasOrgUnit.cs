using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 有所属组织的数据
/// </summary>
/// <remarks>
/// 没有同时实现 IHasOwner 时，“仅本人”权限看不到这类数据
/// </remarks>
public interface IHasOrgUnit
{
    /// <summary>
    /// 所属组织
    /// </summary>
    OrgUnitId OrgUnitId { get; }
}
