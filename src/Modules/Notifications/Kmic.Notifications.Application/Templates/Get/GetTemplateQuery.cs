using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.Templates.Get;

/// <summary>
/// 查询一个通知模板：各渠道当前的内容、代码里的默认内容、变量、示例数据和版本历史
/// </summary>
/// <param name="Key">模板键</param>
[RequirePermission(NotificationsPermissions.TemplateView)]
public sealed record GetTemplateQuery(string Key) : IQuery<TemplateDetailResponse>;
