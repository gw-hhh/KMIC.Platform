using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>
/// 示例：标本提醒的短信
/// </summary>
/// <remarks>
/// 短信服务只接受已审核的模板编码和参数，所以短信模板的模型要实现 ISmsTemplateModel 说明怎样变成参数
/// </remarks>
/// <param name="Number">标本编号</param>
[NotificationTemplate("Sample.SpecimenAlert", "示例标本提醒", Channels = NotificationChannels.Sms)]
public sealed record SpecimenAlertSms(string Number) : INotificationModel<SpecimenAlertSms>, ISmsTemplateModel
{
    /// <inheritdoc />
    public static SpecimenAlertSms Example => new("SP-2026-0001");

    /// <inheritdoc />
    public static string DefaultSubject => "标本提醒";

    /// <inheritdoc />
    public static string DefaultBody => "标本 {{ Number }} 需要处理";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> ToSmsParameters() => new Dictionary<string, string>(StringComparer.Ordinal) { ["number"] = Number };
}
