using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Presentation.Templates;

/// <summary>
/// 发布模板新版本的请求
/// </summary>
/// <param name="Channel">渠道：Email、Sms、InApp，必须是模板声明过的</param>
/// <param name="Subject">主题；站内消息里是标题，最多 200 个字符</param>
/// <param name="Body">正文，最多 20000 个字符</param>
/// <param name="Remark">备注，最多 500 个字符</param>
/// <param name="ExpectedVersion">调用方看到的模板版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record PublishTemplateRequest(TemplateChannel Channel, string Subject, string Body, string? Remark = null, long? ExpectedVersion = null);
