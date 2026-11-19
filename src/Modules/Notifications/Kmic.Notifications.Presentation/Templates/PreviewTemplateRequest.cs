using System.Text.Json;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Presentation.Templates;

/// <summary>
/// 预览模板内容的请求
/// </summary>
/// <param name="Channel">渠道：Email、Sms、InApp</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Model">渲染数据</param>
public sealed record PreviewTemplateRequest(TemplateChannel Channel, string Subject, string Body, JsonElement? Model = null);
