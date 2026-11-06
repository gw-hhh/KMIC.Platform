namespace Spike02.Domain;

/// <summary>
/// 有归属组织单元的聚合（05-05 第 5.2 节）
/// </summary>
public interface IHasOrgUnit
{
    OrgUnitId OrgUnitId { get; }
}
