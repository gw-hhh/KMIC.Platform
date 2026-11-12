using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Sample.Application;

/// <summary>
/// 示例设置：标本提交时的限制
/// </summary>
/// <param name="MaxItems">一个标本最多有多少个明细</param>
/// <param name="ReasonRequired">提交时是否必须写说明</param>
/// <param name="ContactEmail">出问题时联系谁</param>
[SystemSetting("Sample.Submission", "标本提交", Group = "示例", EditPermission = SamplePermissions.SettingManage)]
public sealed record SampleSubmissionSetting(
    [property: SettingField("明细上限", Unit = "个")] int MaxItems = 50,
    [property: SettingField("提交必须写说明")] bool ReasonRequired = false,
    [property: SettingField("联系邮箱")] string? ContactEmail = null) : ISystemSetting<SampleSubmissionSetting>
{
    /// <inheritdoc />
    public static SampleSubmissionSetting Default { get; } = new();

    /// <inheritdoc />
    public static Result Validate(SampleSubmissionSetting value)
    {
        ArgumentNullException.ThrowIfNull(value);

        List<FieldError> problems = [];
        if (value.MaxItems is < 1 or > 500)
        {
            problems.Add(new FieldError("maxItems", "Range", "明细上限要在 1 到 500 之间"));
        }

        if (value.ContactEmail is { Length: > 0 } email && !email.Contains('@', StringComparison.Ordinal))
        {
            problems.Add(new FieldError("contactEmail", "Invalid", "邮箱格式不对"));
        }

        if (problems.Count > 0)
        {
            return CommonErrors.ValidationFailedWith(problems);
        }

        return Result.Success();
    }
}
