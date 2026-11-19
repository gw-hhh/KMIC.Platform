using Kmic.BuildingBlocks.Contracts.Notifications;

namespace Kmic.BuildingBlocks.Application.Notifications;

/// <summary>
/// 代码里声明的一种通知：键、渠道、数据模型、默认内容和示例数据
/// </summary>
/// <param name="Key">模板键</param>
/// <param name="DisplayName">管理页里显示的名称</param>
/// <param name="Module">声明它的模块</param>
/// <param name="Channels">发往哪些渠道</param>
/// <param name="SenderProfile">发件账号名</param>
/// <param name="CarriesSecret">内容含一次性短时凭据</param>
/// <param name="ModelType">数据模型的类型</param>
/// <param name="DefaultSubject">默认主题</param>
/// <param name="DefaultBody">默认正文</param>
/// <param name="DefaultTextBody">短信与站内消息的默认正文</param>
/// <param name="Example">示例数据</param>
/// <param name="Variables">模型里能在模板里用的变量名</param>
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
