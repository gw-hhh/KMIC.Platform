using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Preview;

/// <summary>
/// 预览一段模板内容的渲染结果，不保存
/// </summary>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道，必须是模板声明过的</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Model">渲染数据；未指定时使用模板示例数据，含密模板不得指定</param>
[RequirePermission(NotificationsPermissions.TemplateView)]
public sealed record PreviewTemplateQuery(string Key, TemplateChannel Channel, string Subject, string Body, JsonElement? Model) : IQuery<TemplatePreviewResponse>;
