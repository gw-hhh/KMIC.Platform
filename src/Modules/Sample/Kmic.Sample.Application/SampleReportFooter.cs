using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Sample.Application;

/// <summary>
/// 示例报告页脚偏好，仅允许系统层和角色层设置
/// </summary>
/// <param name="Text">页脚文字</param>
[Preference("Sample.Report.Footer", "报告页脚", Layers = PreferenceLayers.System | PreferenceLayers.Role)]
public sealed record SampleReportFooter(string Text = "") : IPreference<SampleReportFooter>
{
    /// <inheritdoc />
    public static SampleReportFooter Default { get; } = new();

    /// <inheritdoc />
    public static Result Validate(SampleReportFooter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Text.Length <= 100
            ? Result.Success()
            : CommonErrors.ValidationFailedWith([new FieldError("text", "MaxLength", "页脚文字最多 100 个字符")]);
    }
}
