using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Publish;

/// <summary>
/// 发布指定渠道的模板新版本
/// </summary>
/// <remarks>
/// 发布前用示例数据严格渲染，语法错误、遇到未定义变量或超限时拒绝发布；未执行的条件分支不在校验范围。已退役的模板和未声明的渠道不能发布
/// </remarks>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道，必须是模板声明过的</param>
/// <param name="Subject">主题；站内消息里是标题</param>
/// <param name="Body">正文</param>
/// <param name="Remark">备注，最多 500 个字符</param>
/// <param name="ExpectedVersion">模板记录的预期并发版本号，不匹配时返回版本冲突</param>
[RequirePermission(NotificationsPermissions.TemplateManage)]
[Audited]
[Idempotent]
public sealed record PublishTemplateCommand(string Key, TemplateChannel Channel, string Subject, string Body, string? Remark, long? ExpectedVersion)
    : ICommand<VersionedResponse<NotificationTemplateId>>;
