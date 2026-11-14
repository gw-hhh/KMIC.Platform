using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>
/// 示例处理提醒的短信模板
/// </summary>
/// <remarks>
/// 短信仅接受已审核的模板编码和参数，模型通过 ISmsTemplateModel 提供参数映射
/// </remarks>
/// <param name="Number">示例编号</param>
[NotificationTemplate("Sample.SpecimenAlert", "示例处理提醒", Channels = NotificationChannels.Sms)]
public sealed record SpecimenAlertSms(string Number) : INotificationModel<SpecimenAlertSms>, ISmsTemplateModel
{
    /// <inheritdoc />
    public static SpecimenAlertSms Example => new("SP-2026-0001");

    /// <inheritdoc />
    public static string DefaultSubject => "示例处理提醒";

    /// <inheritdoc />
    public static string DefaultBody => "示例 {{ Number }} 需要处理";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> ToSmsParameters() => new Dictionary<string, string>(StringComparer.Ordinal) { ["number"] = Number };
}
