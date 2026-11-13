using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Presentation.Templates;

/// <summary>
/// 恢复模板默认内容的请求
/// </summary>
/// <param name="Channel">渠道：Email、Sms、InApp，必须是模板声明过的</param>
/// <param name="ExpectedVersion">调用方看到的模板版本号，别人改过就返回 409 Common.VersionConflict</param>
public sealed record RevertTemplateRequest(TemplateChannel Channel, long? ExpectedVersion = null);
