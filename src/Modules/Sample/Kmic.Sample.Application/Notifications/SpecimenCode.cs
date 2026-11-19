using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>
/// 示例：含一次性凭据的短信
/// </summary>
/// <remarks>
/// 经 SendSecretNotification 发送
/// </remarks>
/// <param name="Code">验证码</param>
[NotificationTemplate("Sample.SpecimenCode", "示例确认码", Channels = NotificationChannels.Sms, CarriesSecret = true)]
public sealed record SpecimenCode(string Code) : INotificationModel<SpecimenCode>, ISmsTemplateModel
{
    /// <inheritdoc />
    public static SpecimenCode Example => new("123456");

    /// <inheritdoc />
    public static string DefaultSubject => "确认码";

    /// <inheritdoc />
    public static string DefaultBody => "确认码 {{ Code }}，请勿向他人透露";

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> ToSmsParameters() => new Dictionary<string, string>(StringComparer.Ordinal) { ["code"] = Code };
}
