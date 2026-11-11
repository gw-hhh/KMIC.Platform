using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 数据权限授予
/// </summary>
/// <param name="DefaultLevel">默认等级</param>
/// <param name="Overrides">按范围对象单独设置的等级，键为范围对象编码</param>
/// <param name="Anchors">起点组织，为空时从用户所属组织算起</param>
/// <param name="Dimensions">扩展维度，目前为空</param>
public sealed record DataScopeGrant(
    ScopeLevel DefaultLevel,
    IReadOnlyDictionary<string, ScopeLevel> Overrides,
    IReadOnlyList<OrgUnitId> Anchors,
    IReadOnlyDictionary<string, IReadOnlyList<Guid>> Dimensions);
