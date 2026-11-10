using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 有所属组织的资源；只有所属组织没有所属人的资源，“本人”等级看不到任何数据
/// </summary>
public interface IHasOrgUnit
{
    /// <summary>
    /// 所属组织单元
    /// </summary>
    OrgUnitId OrgUnitId { get; }
}
