using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>
/// 示例：标本提交之后通知负责人的邮件与站内消息
/// </summary>
/// <remarks>
/// 演示业务模块怎样声明通知模板：数据模型是不可变的 record，标 NotificationTemplate，属性就是模板里的变量，
/// 默认内容写在模型上，管理员之后可以在界面上改文案。模型里叫 Link 的字符串属性会成为站内消息的跳转链接
/// </remarks>
/// <param name="Number">标本编号</param>
/// <param name="Title">标本标题</param>
/// <param name="Link">站内链接</param>
[NotificationTemplate("Sample.SpecimenSubmitted", "示例标本已提交", Channels = NotificationChannels.Email | NotificationChannels.InApp)]
public sealed record SpecimenSubmittedNotice(string Number, string Title, string Link) : INotificationModel<SpecimenSubmittedNotice>
{
    /// <inheritdoc />
    public static SpecimenSubmittedNotice Example => new("SP-2026-0001", "第一批样品", "/sample/specimens/1");

    /// <inheritdoc />
    public static string DefaultSubject => "示例标本 {{ Number }} 已提交";

    /// <inheritdoc />
    public static string DefaultBody => "<p>标本 {{ Number }}（{{ Title }}）已经提交。</p><p><a href=\"{{ Link }}\">查看详情</a></p>";

    /// <inheritdoc />
    public static string DefaultTextBody => "标本 {{ Number }}（{{ Title }}）已经提交。";
}
