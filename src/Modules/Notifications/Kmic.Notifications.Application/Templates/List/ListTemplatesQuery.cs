using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.List;

/// <summary>查询全部通知模板</summary>
[RequirePermission(NotificationsPermissions.TemplateView)]
public sealed record ListTemplatesQuery : IQuery<TemplateListResponse>;
