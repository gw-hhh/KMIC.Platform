using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates;

/// <summary>
/// 渲染一个渠道的主题和正文：主题是纯文本，邮件正文是 HTML，短信和站内消息的正文是纯文本
/// </summary>
internal static class ChannelRendering
{
    public static async ValueTask<Result<ChannelContent>> RenderAsync(ITemplateRenderer renderer, TemplateChannel channel, string subject, string body, object model, CancellationToken ct)
    {
        var renderedSubject = await renderer.RenderAsync(subject, model, RenderFormat.Text, ct);
        if (renderedSubject.IsFailure)
        {
            return renderedSubject.Error;
        }

        var renderedBody = await renderer.RenderAsync(body, model, channel == TemplateChannel.Email ? RenderFormat.Html : RenderFormat.Text, ct);
        return renderedBody.IsFailure ? renderedBody.Error : new ChannelContent(renderedSubject.Value, renderedBody.Value);
    }
}
