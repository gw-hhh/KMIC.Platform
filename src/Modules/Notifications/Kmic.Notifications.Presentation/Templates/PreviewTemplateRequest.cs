using System.Text.Json;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Presentation.Templates;

/// <summary>
/// 预览模板内容的请求
/// </summary>
/// <param name="Channel">渠道：Email、Sms、InApp，必须是模板声明过的</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Model">用来渲染的数据；不传就用模板的示例数据，含密模板只能用示例数据</param>
public sealed record PreviewTemplateRequest(TemplateChannel Channel, string Subject, string Body, JsonElement? Model = null);
