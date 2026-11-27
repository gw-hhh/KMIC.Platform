using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>数据权限授予信息。</summary>
/// <param name="Anchors">起点组织；为空时以用户所属组织为起点。</param>
/// <param name="Dimensions">扩展维度，当前保留为空。</param>
public sealed record DataScopeGrant(
    ScopeLevel DefaultLevel,
    IReadOnlyDictionary<string, ScopeLevel> Overrides,
    IReadOnlyList<OrgUnitId> Anchors,
    IReadOnlyDictionary<string, IReadOnlyList<Guid>> Dimensions);
