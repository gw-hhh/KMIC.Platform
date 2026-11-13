using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 业务错误
/// </summary>
/// <remarks>
/// 只在各模块的 *Errors 类里创建。实例不可变，可以放在静态字段里共享
/// </remarks>
public sealed partial record Error
{
    private Error(string code, ErrorType type, string message, IReadOnlyList<FieldError> fieldErrors)
    {
        if (!CodePattern().IsMatch(code))
        {
            throw new ArgumentException($"错误码“{code}”不符合“模块.错误名”格式。", nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Code = code;
        Type = type;
        Message = message;
        FieldErrors = fieldErrors;
    }

    /// <summary>
    /// 错误码，如 Sample.NoItems
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 错误类型
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// 提示文字
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 详细说明
    /// </summary>
    public string? Detail { get; private init; }

    /// <summary>
    /// 说明文字的参数
    /// </summary>
    public IReadOnlyList<object?> DetailArgs { get; private init; } = [];

    /// <summary>
    /// 字段错误
    /// </summary>
    public IReadOnlyList<FieldError> FieldErrors { get; }

    /// <summary>
    /// 附加数据
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; private init; } = FrozenDictionary<string, object?>.Empty;

    /// <summary>
    /// 输入不合法
    /// </summary>
    public static Error Validation(string code, string message, IReadOnlyList<FieldError>? fieldErrors = null) =>
        new(code, ErrorType.Validation, message, fieldErrors ?? []);

    /// <summary>
    /// 不存在或不可见
    /// </summary>
    public static Error NotFound(string code, string message) => new(code, ErrorType.NotFound, message, []);

    /// <summary>
    /// 并发、状态或唯一性冲突
    /// </summary>
    public static Error Conflict(string code, string message) => new(code, ErrorType.Conflict, message, []);

    /// <summary>
    /// 已登录但无权执行
    /// </summary>
    public static Error Forbidden(string code, string message) => new(code, ErrorType.Forbidden, message, []);

    /// <summary>
    /// 违反业务规则
    /// </summary>
    public static Error BusinessRule(string code, string message) => new(code, ErrorType.BusinessRule, message, []);

    /// <summary>
    /// 暂时不可用，可以重试
    /// </summary>
    public static Error Unavailable(string code, string message) => new(code, ErrorType.Unavailable, message, []);

    /// <summary>
    /// 凭据无效或没有登录，只用于认证本身
    /// </summary>
    public static Error Unauthenticated(string code, string message) => new(code, ErrorType.Unauthenticated, message, []);

    /// <summary>
    /// 请求过于频繁，调用方要等待 <paramref name="retryAfter" /> 之后再试
    /// </summary>
    public static Error TooManyRequests(string code, string message, TimeSpan retryAfter) =>
        new Error(code, ErrorType.TooManyRequests, message, []).WithMetadata(ErrorMetadataKeys.RetryAfterSeconds, (int)Math.Ceiling(retryAfter.TotalSeconds));

    /// <summary>
    /// 内容太大，超过了允许的上限
    /// </summary>
    public static Error PayloadTooLarge(string code, string message) => new(code, ErrorType.PayloadTooLarge, message, []);

    /// <summary>
    /// 内容类型不被支持
    /// </summary>
    public static Error UnsupportedMediaType(string code, string message) => new(code, ErrorType.UnsupportedMediaType, message, []);

    /// <summary>
    /// 已经不存在，也不会再回来
    /// </summary>
    public static Error Gone(string code, string message) => new(code, ErrorType.Gone, message, []);

    /// <summary>
    /// 附上详细说明
    /// </summary>
    public Error WithDetail(string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        return this with { Detail = detail };
    }

    /// <summary>
    /// 附加一项数据
    /// </summary>
    public Error WithMetadata(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var metadata = new Dictionary<string, object?>(Metadata, StringComparer.Ordinal) { [key] = value };
        return this with { Metadata = metadata.ToFrozenDictionary(StringComparer.Ordinal) };
    }

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*\.[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex CodePattern();
}
