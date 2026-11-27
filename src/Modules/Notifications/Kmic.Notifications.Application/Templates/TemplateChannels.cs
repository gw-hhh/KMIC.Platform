using Kmic.BuildingBlocks.Application.Notifications;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates;

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

    // 邮件默认使用 HTML 正文，短信和站内消息使用纯文本正文
    public static string DefaultBody(NotificationTemplateDefinition definition, TemplateChannel channel)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(channel);

        return channel == TemplateChannel.Email ? definition.DefaultBody : definition.DefaultTextBody;
    }
}
