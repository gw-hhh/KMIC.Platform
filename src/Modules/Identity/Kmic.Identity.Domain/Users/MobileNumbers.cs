using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 手机号的规范化
/// </summary>
/// <remarks>
/// 统一存成国际格式：国内号码写成 +86 加 11 位，其他国家的号码按 E.164 保留。
/// 手机号是登录凭据，也是短信的收件地址，不统一写法会让同一个号码对应多个用户
/// </remarks>
public static partial class MobileNumbers
{
    /// <summary>
    /// 规范化；输入可以带空格、短横线、括号，国内号码可以带 86、0086 或 +86 前缀
    /// </summary>
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

        // 其他国家：已经带国家码的国际格式
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
