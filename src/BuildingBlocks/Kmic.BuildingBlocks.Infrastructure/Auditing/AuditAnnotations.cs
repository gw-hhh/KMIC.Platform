using Kmic.BuildingBlocks.Application.Auditing;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// 属性上的审计设置，存在 EF 模型的注解里
/// </summary>
internal static class AuditAnnotations
{
    public const string Name = "Kmic:AuditMode";

    public const string Hidden = "Hidden";

    public const string Excluded = "Excluded";

    private const string MaskedPrefix = "Masked:";

    public static string Masked(AuditMask mask) => MaskedPrefix + mask;

    public static string? ModeOf(IReadOnlyPropertyBase property) => property.FindAnnotation(Name)?.Value as string;

    public static bool TryReadMask(string? mode, out AuditMask mask)
    {
        mask = default;
        return mode is not null && mode.StartsWith(MaskedPrefix, StringComparison.Ordinal) && Enum.TryParse(mode[MaskedPrefix.Length..], out mask);
    }
}
