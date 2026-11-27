using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Presentation.Templates;

/// <param name="Channel">渠道：Email、Sms、InApp</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Remark">备注</param>
/// <param name="ExpectedVersion">模板记录的预期并发版本号</param>
public sealed record PublishTemplateRequest(TemplateChannel Channel, string Subject, string Body, string? Remark = null, long? ExpectedVersion = null);
