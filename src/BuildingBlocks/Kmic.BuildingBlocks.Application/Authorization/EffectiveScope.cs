using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 有效数据范围
/// </summary>
/// <param name="All">是否不受限</param>
/// <param name="Owner">本人，不受限时为空</param>
/// <param name="OrgUnits">可见的组织</param>
public sealed record EffectiveScope(bool All, UserId? Owner, IReadOnlyList<OrgUnitId> OrgUnits)
{
    /// <summary>
    /// 不受限
    /// </summary>
    public static EffectiveScope Unrestricted { get; } = new(true, null, []);
}
