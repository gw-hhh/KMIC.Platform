using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Compliance.Classification;

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

    // Internal 完整记录，不需要注解
    public static string? For(DataClassification classification)
    {
        if (classification == KmicData.Secret)
        {
            return Hidden;
        }

        if (classification == KmicData.Personal)
        {
            return Masked(AuditMask.Personal);
        }

        return classification == KmicData.Internal ? null : throw new ArgumentOutOfRangeException(nameof(classification), classification, "不是 Kmic 的数据分类。");
    }

    public static string? ModeOf(IReadOnlyPropertyBase property) => property.FindAnnotation(Name)?.Value as string;

    public static bool TryReadMask(string? mode, out AuditMask mask)
    {
        mask = default;
        return mode is not null && mode.StartsWith(MaskedPrefix, StringComparison.Ordinal) && Enum.TryParse(mode[MaskedPrefix.Length..], out mask);
    }
}
