using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Serialization;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// 审计记录里字段值的写法
/// </summary>
internal static class AuditValues
{
    /// <summary>
    /// 把存储值转成字符串：时刻用 ISO 8601，数字和布尔用不受区域影响的写法，其余按系统的 JSON 写法
    /// </summary>
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

    /// <summary>
    /// 超过最大长度时截断，并附上完整值的哈希前 8 位，方便判断两个被截断的值是否相同
    /// </summary>
    public static string? Truncate(string? value, int maxLength)
    {
        if (value is null || value.Length <= maxLength)
        {
            return value;
        }

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..8];
        return $"{value[..maxLength]}…(sha256:{hash})";
    }

    /// <summary>
    /// 按遮盖方式处理，规则与日志共用 <see cref="PartialMask" />
    /// </summary>
    public static string Mask(string value, AuditMask mask)
    {
        ArgumentNullException.ThrowIfNull(value);
        return mask switch
        {
            AuditMask.Email => PartialMask.Email(value),
            AuditMask.Phone or AuditMask.IdNumber => PartialMask.Middle(value),
            AuditMask.Personal => PartialMask.Of(value),
            _ => throw new ArgumentOutOfRangeException(nameof(mask), mask, "没有这种遮盖方式。"),
        };
    }
}
