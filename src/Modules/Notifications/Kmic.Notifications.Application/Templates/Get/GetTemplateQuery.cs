using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.Get;

/// <summary>
/// 查询通知模板详情
/// </summary>
[RequirePermission(NotificationsPermissions.TemplateView)]
public sealed record GetTemplateQuery(string Key) : IQuery<TemplateDetailResponse>;
