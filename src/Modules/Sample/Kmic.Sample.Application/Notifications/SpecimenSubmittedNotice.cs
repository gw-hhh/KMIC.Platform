using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Sample.Application.Notifications;

/// <summary>
/// 示例提交后通知负责人的邮件与站内消息模板
/// </summary>
/// <remarks>
/// record 的属性就是模板变量
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
