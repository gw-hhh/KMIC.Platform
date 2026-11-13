using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Revert;

/// <summary>
/// 恢复模板某个渠道的默认内容
/// </summary>
/// <remarks>
/// 这个渠道改回用代码里的内容，发布过的历史版本保留。本来就在用默认内容的渠道再恢复一次，什么也不改
/// </remarks>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道，必须是模板声明过的</param>
/// <param name="ExpectedVersion">调用方看到的模板版本号，别人改过就返回版本冲突</param>
[RequirePermission(NotificationsPermissions.TemplateManage)]
[Audited]
[Idempotent]
public sealed record RevertTemplateCommand(string Key, TemplateChannel Channel, long? ExpectedVersion)
    : ICommand<VersionedResponse<NotificationTemplateId>>;
