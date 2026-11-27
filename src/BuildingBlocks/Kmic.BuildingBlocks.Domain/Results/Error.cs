using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>业务错误描述（仅限在各模块的 *Errors 静态类中创建）。</summary>
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
    /// 错误码（例如 Sample.NoItems）
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 错误类型
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// 面向调用方的提示信息
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 错误详情说明
    /// </summary>
    public string? Detail { get; private init; }

    /// <summary>
    /// 错误详情模板参数
    /// </summary>
    public IReadOnlyList<object?> DetailArgs { get; private init; } = [];

    /// <summary>
    /// 字段级校验错误列表
    /// </summary>
    public IReadOnlyList<FieldError> FieldErrors { get; }

    /// <summary>
    /// 附加元数据
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; private init; } = FrozenDictionary<string, object?>.Empty;

    /// <summary>创建参数校验失败错误（HTTP 400）。</summary>
    public static Error Validation(string code, string message, IReadOnlyList<FieldError>? fieldErrors = null) =>
        new(code, ErrorType.Validation, message, fieldErrors ?? []);

    /// <summary>创建资源不存在或不可见错误（HTTP 404）。</summary>
    public static Error NotFound(string code, string message) => new(code, ErrorType.NotFound, message, []);

    /// <summary>创建并发、状态或唯一性冲突错误（HTTP 409）。</summary>
    public static Error Conflict(string code, string message) => new(code, ErrorType.Conflict, message, []);

    /// <summary>创建无权限错误（HTTP 403）。</summary>
    public static Error Forbidden(string code, string message) => new(code, ErrorType.Forbidden, message, []);

    /// <summary>创建违反业务规则错误（HTTP 422）。</summary>
    public static Error BusinessRule(string code, string message) => new(code, ErrorType.BusinessRule, message, []);

    /// <summary>创建服务暂不可用错误（HTTP 503，可重试）。</summary>
    public static Error Unavailable(string code, string message) => new(code, ErrorType.Unavailable, message, []);

    /// <summary>创建未认证或凭据无效错误（HTTP 401，仅用于认证流程）。</summary>
    public static Error Unauthenticated(string code, string message) => new(code, ErrorType.Unauthenticated, message, []);

    /// <summary>创建限流错误（HTTP 429），需等待 <paramref name="retryAfter" /> 后重试。</summary>
    public static Error TooManyRequests(string code, string message, TimeSpan retryAfter) =>
        new Error(code, ErrorType.TooManyRequests, message, []).WithMetadata(ErrorMetadataKeys.RetryAfterSeconds, (int)Math.Ceiling(retryAfter.TotalSeconds));

    /// <summary>创建请求内容过大错误（HTTP 413）。</summary>
    public static Error PayloadTooLarge(string code, string message) => new(code, ErrorType.PayloadTooLarge, message, []);

    /// <summary>创建不支持的媒体类型错误（HTTP 415）。</summary>
    public static Error UnsupportedMediaType(string code, string message) => new(code, ErrorType.UnsupportedMediaType, message, []);

    /// <summary>创建资源已永久移除错误（HTTP 410）。</summary>
    public static Error Gone(string code, string message) => new(code, ErrorType.Gone, message, []);

    /// <summary>附加错误详情说明。</summary>
    public Error WithDetail(string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        return this with { Detail = detail };
    }

    /// <summary>附加自定义元数据项。</summary>
    public Error WithMetadata(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var metadata = new Dictionary<string, object?>(Metadata, StringComparer.Ordinal) { [key] = value };
        return this with { Metadata = metadata.ToFrozenDictionary(StringComparer.Ordinal) };
    }

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*\.[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex CodePattern();
}
