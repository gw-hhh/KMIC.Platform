using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>
/// 示例提交后通知负责人的邮件与站内消息模板
/// </summary>
/// <remarks>
/// 演示业务模块声明通知模板：不可变 record 的属性提供模板变量，默认内容由模型定义，管理员可另行发布版本。
/// Link 字符串属性用于站内消息跳转
/// </remarks>
/// <param name="Number">示例编号</param>
/// <param name="Title">示例标题</param>
/// <param name="Link">站内链接</param>
[NotificationTemplate("Sample.SpecimenSubmitted", "示例已提交", Channels = NotificationChannels.Email | NotificationChannels.InApp)]
public sealed record SpecimenSubmittedNotice(string Number, string Title, string Link) : INotificationModel<SpecimenSubmittedNotice>
{
    /// <inheritdoc />
    public static SpecimenSubmittedNotice Example => new("SP-2026-0001", "第一批样品", "/sample/specimens/1");

    /// <inheritdoc />
    public static string DefaultSubject => "示例 {{ Number }} 已提交";

    /// <inheritdoc />
    public static string DefaultBody => "<p>示例 {{ Number }}（{{ Title }}）已提交。</p><p><a href=\"{{ Link }}\">查看详情</a></p>";

    /// <inheritdoc />
    public static string DefaultTextBody => "示例 {{ Number }}（{{ Title }}）已提交。";
}
