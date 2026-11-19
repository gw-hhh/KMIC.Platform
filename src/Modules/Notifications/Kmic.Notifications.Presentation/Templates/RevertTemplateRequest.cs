using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Presentation.Templates;

/// <summary>
/// 恢复模板默认内容的请求
/// </summary>
/// <param name="Channel">渠道：Email、Sms、InApp</param>
/// <param name="ExpectedVersion">模板记录的预期并发版本号</param>
public sealed record RevertTemplateRequest(TemplateChannel Channel, long? ExpectedVersion = null);
