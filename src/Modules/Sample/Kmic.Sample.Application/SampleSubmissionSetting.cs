using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Sample.Application;

/// <summary>
/// 示例：声明并校验提交设置
/// </summary>
/// <remarks>
/// 当前创建和提交命令未读取这些设置，不据此限制明细数量或要求提交说明
/// </remarks>
/// <param name="MaxItems">明细数量上限</param>
/// <param name="ReasonRequired">提交时是否必须写说明</param>
/// <param name="ContactEmail">联系邮箱</param>
[SystemSetting("Sample.Submission", "示例提交", Group = "示例", EditPermission = SamplePermissions.SettingManage)]
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
            problems.Add(new FieldError("contactEmail", "Invalid", "邮箱格式不正确"));
        }

        if (problems.Count > 0)
        {
            return CommonErrors.ValidationFailedWith(problems);
        }

        return Result.Success();
    }
}
