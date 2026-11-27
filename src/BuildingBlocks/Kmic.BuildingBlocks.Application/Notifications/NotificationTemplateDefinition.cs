using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.BuildingBlocks.Application.Notifications;

/// <summary>通知模板定义。</summary>
/// <param name="SenderProfile">发件账号配置名</param>
/// <param name="CarriesSecret">内容是否包含一次性短时凭据</param>
/// <param name="DefaultTextBody">短信与站内消息的默认正文</param>
/// <param name="Variables">模板可用的模型变量名列表</param>
public sealed record NotificationTemplateDefinition(
    string Key,
    string DisplayName,
    string Module,
    NotificationChannels Channels,
    string SenderProfile,
    bool CarriesSecret,
    Type ModelType,
    string DefaultSubject,
    string DefaultBody,
    string DefaultTextBody,
    object Example,
    IReadOnlyList<string> Variables);
