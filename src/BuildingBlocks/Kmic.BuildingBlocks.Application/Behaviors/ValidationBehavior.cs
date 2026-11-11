using System.Collections.Frozen;
using FluentValidation;
using FluentValidation.Results;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 请求校验
/// </summary>
/// <remarks>
/// 校验器只检查格式和取值范围，不读数据库或其他模块
/// </remarks>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    // 内置规则的错误码换成短码，自定义规则用 WithErrorCode 写明
    private static readonly FrozenDictionary<string, string> _builtInCodes = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["NotNullValidator"] = "Required",
        ["NotEmptyValidator"] = "Required",
        ["MaximumLengthValidator"] = "MaxLength",
        ["MinimumLengthValidator"] = "MinLength",
        ["LengthValidator"] = "Length",
        ["ExactLengthValidator"] = "Length",
        ["GreaterThanValidator"] = "Range",
        ["GreaterThanOrEqualValidator"] = "Range",
        ["LessThanValidator"] = "Range",
        ["LessThanOrEqualValidator"] = "Range",
        ["InclusiveBetweenValidator"] = "Range",
        ["ExclusiveBetweenValidator"] = "Range",
        ["EmailValidator"] = "Format",
        ["RegularExpressionValidator"] = "Format",
        ["EnumValidator"] = "Format",
        ["EmptyValidator"] = "MustBeEmpty",
        ["NullValidator"] = "MustBeEmpty",
        ["EqualValidator"] = "Equal",
        ["NotEqualValidator"] = "NotEqual",
        ["ScalePrecisionValidator"] = "Precision",
        ["PrecisionScaleValidator"] = "Precision",
    }.ToFrozenDictionary(StringComparer.Ordinal);

    private readonly IValidator<TMessage>[] _validators = [.. validators];

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);
        if (_validators.Length == 0)
        {
            return await next(message, cancellationToken);
        }

        var failures = new List<ValidationFailure>();
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(new ValidationContext<TMessage>(message), cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        FieldError[] fieldErrors = [.. failures.Select(f => new FieldError(ToJsonPath(f.PropertyName), ToCode(f.ErrorCode), f.ErrorMessage))];
        return TResponse.Fail(CommonErrors.ValidationFailedWith(fieldErrors));
    }

    // Items[2].Quantity 转成 items[2].quantity，和请求 JSON 一致
    internal static string ToJsonPath(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(segment => segment.Length == 0 ? segment : char.ToLowerInvariant(segment[0]) + segment[1..]));

    private static string ToCode(string errorCode) =>
        _builtInCodes.TryGetValue(errorCode, out var code) ? code : TrimValidatorSuffix(errorCode);

    // 没有对应短码的内置规则去掉 Validator 后缀，自定义规则原样保留
    private static string TrimValidatorSuffix(string errorCode) =>
        errorCode.EndsWith("Validator", StringComparison.Ordinal) ? errorCode[..^"Validator".Length] : errorCode;
}
