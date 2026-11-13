using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates;

/// <summary>
/// 代码里声明的渠道组合和模板渠道之间的对应
/// </summary>
internal static class TemplateChannels
{
    public static IReadOnlyList<TemplateChannel> From(NotificationChannels channels)
    {
        var result = new List<TemplateChannel>();
        if (channels.HasFlag(NotificationChannels.Email))
        {
            result.Add(TemplateChannel.Email);
        }

        if (channels.HasFlag(NotificationChannels.Sms))
        {
            result.Add(TemplateChannel.Sms);
        }

        if (channels.HasFlag(NotificationChannels.InApp))
        {
            result.Add(TemplateChannel.InApp);
        }

        return result;
    }

    /// <summary>
    /// 代码里声明的某个渠道的默认正文：邮件是 HTML，短信和站内消息是纯文本
    /// </summary>
    public static string DefaultBody(NotificationTemplateDefinition definition, TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(channel);

        return channel == TemplateChannel.Email ? definition.DefaultBody : definition.DefaultTextBody;
    }
}
