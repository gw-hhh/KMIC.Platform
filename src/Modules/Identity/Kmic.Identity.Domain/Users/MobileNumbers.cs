using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 手机号规范化：大陆号码统一加 +86，其他按国际格式校验
/// </summary>
public static partial class MobileNumbers
{
    /// <summary>
    /// 校验并规范化手机号
    /// </summary>
    /// <remarks>
    /// 号码里可以有空白、短横线和括号；大陆号码可带 86、0086 或 +86 前缀
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
