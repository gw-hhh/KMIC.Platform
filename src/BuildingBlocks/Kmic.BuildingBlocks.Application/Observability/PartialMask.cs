namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 部分遮盖：保留能认出来的头尾，中间换成星号
/// </summary>
/// <remarks>
/// 审计记录和日志用同一套规则，同一个手机号在两处看到的一样
/// </remarks>
public static class PartialMask
{
    /// <summary>
    /// 保留前 3 位和后 4 位，太短的整个遮住，如 138****5678
    /// </summary>
    public static string Middle(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        const int Head = 3;
        const int Tail = 4;
        return value.Length <= Head + Tail
            ? new string('*', value.Length)
            : value[..Head] + new string('*', value.Length - Head - Tail) + value[^Tail..];
    }

    /// <summary>
    /// 保留邮箱的第 1 个字符和域名，如 z***@example.com；不是邮箱的整个遮住
    /// </summary>
    public static string Email(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var at = value.IndexOf('@', StringComparison.Ordinal);
        return at <= 0 ? new string('*', value.Length) : value[0] + "***" + value[at..];
    }

    /// <summary>
    /// 按内容判断：含 @ 的当邮箱，其余按手机号、证件号处理
    /// </summary>
    public static string Of(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Contains('@', StringComparison.Ordinal) ? Email(value) : Middle(value);
    }
}
