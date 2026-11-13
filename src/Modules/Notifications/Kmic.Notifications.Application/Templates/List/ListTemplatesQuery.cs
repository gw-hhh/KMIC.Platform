using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.List;

/// <summary>
/// 查询全部通知模板：渠道、状态、各渠道生效的版本、最近一次发布的时间
/// </summary>
[RequirePermission(NotificationsPermissions.TemplateView)]
public sealed record ListTemplatesQuery : IQuery<TemplateListResponse>;
