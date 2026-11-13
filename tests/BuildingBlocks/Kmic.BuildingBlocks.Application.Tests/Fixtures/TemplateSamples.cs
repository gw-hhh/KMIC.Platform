using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

/// <summary>
/// 通知模板的声明，有对的也有故意写错的，交给 NotificationTemplateCatalog 校验
/// </summary>
public static class TemplateSamples
{
    [NotificationTemplate("BuildingBlocks.Approved", "已审批", Channels = NotificationChannels.Email | NotificationChannels.InApp)]
    public sealed record Approved(string Number, string Approver) : INotificationModel<Approved>
    {
        public static Approved Example => new("S-1", "张三");

        public static string DefaultSubject => "单据 {{ Number }} 已通过";

        public static string DefaultBody => "<p>{{ Approver }} 通过了 {{ Number }}</p>";

        public static string DefaultTextBody => "{{ Approver }} 通过了 {{ Number }}";
    }

    [NotificationTemplate("BuildingBlocks.Code", "验证码", Channels = NotificationChannels.Sms, CarriesSecret = true)]
    public sealed record Code(string Value) : INotificationModel<Code>, ISmsTemplateModel
    {
        public static Code Example => new("123456");

        public static string DefaultSubject => "验证码";

        public static string DefaultBody => "验证码 {{ Value }}，5 分钟内有效";

        public IReadOnlyDictionary<string, string> ToSmsParameters() => new Dictionary<string, string>(StringComparer.Ordinal) { ["code"] = Value };
    }

    [NotificationTemplate("Other.WrongModule", "键不以模块名开头", Channels = NotificationChannels.InApp)]
    public sealed record WrongPrefix(string Text) : INotificationModel<WrongPrefix>
    {
        public static WrongPrefix Example => new("a");

        public static string DefaultSubject => "a";

        public static string DefaultBody => "a";
    }

    [NotificationTemplate("BuildingBlocks.NoChannel", "没有渠道")]
    public sealed record NoChannel(string Text) : INotificationModel<NoChannel>
    {
        public static NoChannel Example => new("a");

        public static string DefaultSubject => "a";

        public static string DefaultBody => "a";
    }

    [NotificationTemplate("BuildingBlocks.SecretMail", "含密还发邮件", Channels = NotificationChannels.Email, CarriesSecret = true)]
    public sealed record SecretMail(string Text) : INotificationModel<SecretMail>
    {
        public static SecretMail Example => new("a");

        public static string DefaultSubject => "a";

        public static string DefaultBody => "a";
    }

    [NotificationTemplate("BuildingBlocks.SmsWithoutMapping", "短信没有参数映射", Channels = NotificationChannels.Sms)]
    public sealed record SmsWithoutMapping(string Text) : INotificationModel<SmsWithoutMapping>
    {
        public static SmsWithoutMapping Example => new("a");

        public static string DefaultSubject => "a";

        public static string DefaultBody => "a";
    }

    [NotificationTemplate("BuildingBlocks.NotAModel", "不是通知模型", Channels = NotificationChannels.InApp)]
    public sealed record NotAModel(string Text);

    [NotificationTemplate("BuildingBlocks.Approved", "和第一个重复", Channels = NotificationChannels.InApp)]
    public sealed record DuplicateKey(string Text) : INotificationModel<DuplicateKey>
    {
        public static DuplicateKey Example => new("a");

        public static string DefaultSubject => "a";

        public static string DefaultBody => "a";
    }
}
