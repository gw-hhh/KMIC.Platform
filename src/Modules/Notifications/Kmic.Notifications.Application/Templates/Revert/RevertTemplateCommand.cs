using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Revert;

/// <summary>恢复指定渠道的模板默认内容</summary>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道</param>
/// <param name="ExpectedVersion">模板记录的预期并发版本号</param>
[RequirePermission(NotificationsPermissions.TemplateManage)]
[Audited]
[Idempotent]
public sealed record RevertTemplateCommand(string Key, TemplateChannel Channel, long? ExpectedVersion)
    : ICommand<VersionedResponse<NotificationTemplateId>>;
