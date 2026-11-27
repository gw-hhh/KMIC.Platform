using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Serialization;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

internal static class AuditValues
{
    public static string? Format(object? stored) => stored switch
    {
        null => null,
        string text => text,
        bool flag => flag ? "true" : "false",
        Guid id => id.ToString("D"),
        DateTimeOffset instant => instant.ToString("O", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("O", CultureInfo.InvariantCulture),
        TimeOnly time => time.ToString("O", CultureInfo.InvariantCulture),
        IFormattable number when stored.GetType().IsPrimitive || stored is decimal => number.ToString(null, CultureInfo.InvariantCulture),
        Enum value => value.ToString(),
        _ => JsonSerializer.Serialize(stored, stored.GetType(), KmicJsonOptions.Stored),
    };

    // 超长截断并附带完整值 SHA-256 前 8 位，便于比对一致性
    public static string? Truncate(string? value, int maxLength)
    {
        if (value is null || value.Length <= maxLength)
        {
            return value;
        }

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..8];
        return $"{value[..maxLength]}…(sha256:{hash})";
    }

    public static string Mask(string value, AuditMask mask)
    {
        ArgumentNullException.ThrowIfNull(value);
        return mask switch
        {
            AuditMask.Email => PartialMask.Email(value),
            AuditMask.Phone or AuditMask.IdNumber => PartialMask.Middle(value),
            AuditMask.Personal => PartialMask.Of(value),
            _ => throw new ArgumentOutOfRangeException(nameof(mask), mask, "不支持的脱敏方式。"),
        };
    }
}
