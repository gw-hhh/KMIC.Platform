using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Sample.Application;

/// <summary>
/// 示例偏好：标本列表每页多少条，系统、角色、本人三层都可以设置
/// </summary>
/// <param name="Value">每页条数</param>
[Preference("Sample.List.PageSize", "标本列表每页条数")]
public sealed record SampleListPageSize(int Value = 20) : IPreference<SampleListPageSize>
{
    /// <inheritdoc />
    public static SampleListPageSize Default { get; } = new();

    /// <inheritdoc />
    public static Result Validate(SampleListPageSize value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Value is >= 10 and <= 200
            ? Result.Success()
            : CommonErrors.ValidationFailedWith([new FieldError("value", "Range", "每页条数要在 10 到 200 之间")]);
    }
}
