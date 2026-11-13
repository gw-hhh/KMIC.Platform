using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates.Publish;

/// <summary>
/// 给模板的一个渠道发布新版本，立即生效
/// </summary>
/// <remarks>
/// 发布前先用示例数据渲染一遍，语法错误、超时、输出过大都在这里拦下。已退役的模板和模板没有的渠道不能发布
/// </remarks>
/// <param name="Key">模板键</param>
/// <param name="Channel">渠道，必须是模板声明过的</param>
/// <param name="Subject">主题；站内消息里是标题</param>
/// <param name="Body">正文</param>
/// <param name="Remark">备注，最多 500 个字符</param>
/// <param name="ExpectedVersion">调用方看到的模板版本号，别人改过就返回版本冲突</param>
[RequirePermission(NotificationsPermissions.TemplateManage)]
[Audited]
[Idempotent]
public sealed record PublishTemplateCommand(string Key, TemplateChannel Channel, string Subject, string Body, string? Remark, long? ExpectedVersion)
    : ICommand<VersionedResponse<NotificationTemplateId>>;
