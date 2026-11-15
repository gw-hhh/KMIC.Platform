using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 手机号的规范化
/// </summary>
/// <remarks>
/// 大陆手机号统一加 +86，其他号码按国际号码格式校验。
/// </remarks>
public static partial class MobileNumbers
{
    /// <summary>
    /// 校验并规范化手机号
    /// </summary>
    /// <remarks>
    /// 允许号码中含空白、短横线和括号；大陆号码可带 86、0086 或 +86 前缀。
    /// </remarks>
    public static Result<string> Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return IdentityErrors.MobileInvalid;
        }

        var compact = Separators().Replace(input, string.Empty);
        if (compact.StartsWith("00", StringComparison.Ordinal))
        {
            compact = "+" + compact[2..];
        }

        if (MainlandWithoutPrefix().IsMatch(compact))
        {
            return "+86" + compact;
        }

        if (MainlandWithPrefix().IsMatch(compact))
        {
            return "+86" + compact[^11..];
        }

        // 其他号码须已包含国际区号
        return International().IsMatch(compact) && !compact.StartsWith("+86", StringComparison.Ordinal) ? compact : IdentityErrors.MobileInvalid;
    }

    [GeneratedRegex(@"[\s\-()]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex Separators();

    [GeneratedRegex(@"^1[3-9]\d{9}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex MainlandWithoutPrefix();

    [GeneratedRegex(@"^(?:\+86|86)1[3-9]\d{9}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex MainlandWithPrefix();

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex International();
}
