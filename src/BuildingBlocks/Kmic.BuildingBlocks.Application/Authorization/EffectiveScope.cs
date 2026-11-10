using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 当前请求在某个范围对象上的有效范围，由 EF 命名过滤器转为参数化条件
/// </summary>
/// <param name="All">是否不受限</param>
/// <param name="Owner">“本人”对应的用户；All 为 true 时为空</param>
/// <param name="OrgUnits">已展开的组织单元集合</param>
public sealed record EffectiveScope(bool All, UserId? Owner, IReadOnlyList<OrgUnitId> OrgUnits)
{
    /// <summary>
    /// 不受限
    /// </summary>
    public static EffectiveScope Unrestricted { get; } = new(true, null, []);
}
