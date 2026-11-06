namespace Spike03.Domain;

/// <summary>
/// 单值值对象：映射为一列
/// </summary>
public sealed record EmailAddress : ISingleValueObject<string>
{
    private EmailAddress(string value) => Value = value;

    public string Value { get; private init; }

    public static EmailAddress Create(string input)
    {
        var normalized = input.Trim().ToLowerInvariant();
        return normalized.Contains('@', StringComparison.Ordinal)
            ? new EmailAddress(normalized)
            : throw new ArgumentException("邮箱格式不正确", nameof(input));
    }
}
