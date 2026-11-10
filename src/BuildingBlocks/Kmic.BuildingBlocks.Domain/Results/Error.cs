using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 一次业务失败的描述：稳定的错误码、大类和给用户看的文字
/// </summary>
/// <remarks>
/// 业务代码只能在 *Errors 静态类（错误目录）中创建它。实例不可变，WithDetail、WithMetadata 返回新实例，
/// 所以错误目录里的静态字段可以放心共享。
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
    /// 错误码，格式为“模块.错误名”，如 Sample.NoItems；前端只依据它做判断
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// 错误大类
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// 固定的提示文字，对应 ProblemDetails 的 title
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 带具体参数的说明，对应 ProblemDetails 的 detail；没有时为空
    /// </summary>
    public string? Detail { get; private init; }

    /// <summary>
    /// 说明文字的参数，为将来的多语言预留，目前总是空集合
    /// </summary>
    public IReadOnlyList<object?> DetailArgs { get; private init; } = [];

    /// <summary>
    /// 字段错误，只有 Validation 类错误才有内容
    /// </summary>
    public IReadOnlyList<FieldError> FieldErrors { get; }

    /// <summary>
    /// 框架附加的数据，键见 ErrorMetadataKeys
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
    /// 可重试的瞬时故障；业务错误目录不得使用
    /// </summary>
    public static Error Unavailable(string code, string message) => new(code, ErrorType.Unavailable, message, []);

    /// <summary>
    /// 返回带具体说明的新实例
    /// </summary>
    public Error WithDetail(string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        return this with { Detail = detail };
    }

    /// <summary>
    /// 返回附加了一项框架数据的新实例
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
