using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <param name="Owner">本人标识；不受限时为 null。</param>
public sealed record EffectiveScope(bool All, UserId? Owner, IReadOnlyList<OrgUnitId> OrgUnits)
{
    public static EffectiveScope Unrestricted { get; } = new(true, null, []);
}
