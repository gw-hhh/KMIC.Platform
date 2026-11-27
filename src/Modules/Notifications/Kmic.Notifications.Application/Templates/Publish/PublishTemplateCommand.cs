using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Publish;

/// <summary>发布指定渠道的模板新版本（发布前使用示例数据严格校验）</summary>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="Remark">备注</param>
/// <param name="ExpectedVersion">模板记录的预期并发版本号</param>
[RequirePermission(NotificationsPermissions.TemplateManage)]
[Audited]
[Idempotent]
public sealed record PublishTemplateCommand(string Key, TemplateChannel Channel, string Subject, string Body, string? Remark, long? ExpectedVersion)
    : ICommand<VersionedResponse<NotificationTemplateId>>;
