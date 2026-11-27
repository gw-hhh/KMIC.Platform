using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>示例处理提醒短信模板</summary>
/// <param name="Number">示例编号</param>
[NotificationTemplate("Sample.SpecimenAlert", "示例处理提醒", Channels = NotificationChannels.Sms)]
public sealed record SpecimenAlertSms(string Number) : INotificationModel<SpecimenAlertSms>, ISmsTemplateModel
{
    public static SpecimenAlertSms Example => new("SP-2026-0001");

    public static string DefaultSubject => "示例处理提醒";

    public static string DefaultBody => "示例 {{ Number }} 需要处理";

    public IReadOnlyDictionary<string, string> ToSmsParameters() => new Dictionary<string, string>(StringComparer.Ordinal) { ["number"] = Number };
}
