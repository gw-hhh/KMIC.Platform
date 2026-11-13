using System.Text.Json;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Preview;

/// <summary>
/// 预览一段模板内容的渲染结果，不保存
/// </summary>
/// <remarks>
/// 渲染的限时和输出上限与正式处理一样。模板有误时不当作调用失败，错误原因放在响应里，方便边写边看
/// </remarks>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道，必须是模板声明过的</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Model">用来渲染的数据；不传就用模板的示例数据，含密模板只能用示例数据</param>
[RequirePermission(NotificationsPermissions.TemplateView)]
public sealed record PreviewTemplateQuery(string Key, TemplateChannel Channel, string Subject, string Body, JsonElement? Model) : IQuery<TemplatePreviewResponse>;
