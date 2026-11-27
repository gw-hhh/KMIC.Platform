using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.Notifications.Application.BackgroundJobs;

/// <summary>后台作业结束时发送给提交人的站内消息模板</summary>
/// <param name="JobName">作业显示名称</param>
/// <param name="Result">结果</param>
/// <param name="ErrorMessage">失败原因，未失败时为空字符串</param>
[NotificationTemplate(TemplateKey, "后台作业结束", Channels = NotificationChannels.InApp)]
public sealed record BackgroundJobFinishedNotice(string JobName, string Result, string ErrorMessage) : INotificationModel<BackgroundJobFinishedNotice>
{
    public const string TemplateKey = "Notifications.BackgroundJobFinished";

    public static BackgroundJobFinishedNotice Example => new("审计导出", "失败", "导出的记录超过上限。");

    public static string DefaultSubject => "{{ JobName }}{{ Result }}";

    public static string DefaultBody => "后台作业“{{ JobName }}”{{ Result }}。{% if ErrorMessage != \"\" %}原因：{{ ErrorMessage }}{% endif %}";
}
