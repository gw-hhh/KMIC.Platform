using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 当前角色在数据范围上的授予：默认等级、按范围对象的覆盖与授予锚点
/// </summary>
/// <param name="DefaultLevel">角色的默认等级，适用于所有范围对象</param>
/// <param name="Overrides">按范围对象覆盖的等级，键为范围对象名</param>
/// <param name="Anchors">授予上的锚点组织单元；为空时以用户所属组织为起点</param>
/// <param name="Dimensions">扩展维度的取值（05-05 第 5.9 节），目前为空</param>
public sealed record DataScopeGrant(
    ScopeLevel DefaultLevel,
    IReadOnlyDictionary<string, ScopeLevel> Overrides,
    IReadOnlyList<OrgUnitId> Anchors,
    IReadOnlyDictionary<string, IReadOnlyList<Guid>> Dimensions);
