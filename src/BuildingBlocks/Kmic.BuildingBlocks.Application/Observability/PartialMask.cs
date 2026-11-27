namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>部分脱敏工具，保留首尾字符并用星号遮盖中间部分。</summary>
public static class PartialMask
{
    /// <summary>保留前 3 位与后 4 位（如 138****5678），过短时全量遮盖。</summary>
    public static string Middle(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        const int Head = 3;
        const int Tail = 4;
        return value.Length <= Head + Tail
            ? new string('*', value.Length)
            : value[..Head] + new string('*', value.Length - Head - Tail) + value[^Tail..];
    }

    /// <summary>保留邮箱首字符与域名（如 z***@example.com），非邮箱格式全量遮盖。</summary>
    public static string Email(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var at = value.IndexOf('@', StringComparison.Ordinal);
        return at <= 0 ? new string('*', value.Length) : value[0] + "***" + value[at..];
    }

    /// <summary>按内容自动识别：含 @ 按邮箱处理，其余按手机号或证件号处理。</summary>
    public static string Of(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Contains('@', StringComparison.Ordinal) ? Email(value) : Middle(value);
    }
}
