using Spike02.Domain;

namespace Spike02.Infrastructure;

/// <summary>
/// 某个范围对象上的有效范围（05-05 第 5.3 节）
/// </summary>
public sealed record EffectiveScope(bool All, UserId? Owner, IReadOnlyList<OrgUnitId> OrgUnits);
