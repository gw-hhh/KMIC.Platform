using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Application.Rendering;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Templates;

/// <summary>
/// 按渠道渲染通知主题和正文：邮件正文按 HTML，其余按纯文本
/// </summary>
internal static class ChannelRendering
{
    public static async ValueTask<Result<ChannelContent>> RenderAsync(ITemplateRenderer renderer, TemplateChannel channel, string subject, string body, object model, bool strict, CancellationToken ct)
    {
        var renderedSubject = await RenderOneAsync(renderer, subject, model, RenderFormat.Text, strict, ct);
        if (renderedSubject.IsFailure)
        {
            return renderedSubject.Error;
        }

        var renderedBody = await RenderOneAsync(renderer, body, model, channel == TemplateChannel.Email ? RenderFormat.Html : RenderFormat.Text, strict, ct);
        return renderedBody.IsFailure ? renderedBody.Error : new ChannelContent(renderedSubject.Value, renderedBody.Value);
    }

    private static ValueTask<Result<string>> RenderOneAsync(ITemplateRenderer renderer, string text, object model, RenderFormat format, bool strict, CancellationToken ct) =>
        strict ? renderer.RenderStrictAsync(text, model, format, ct) : renderer.RenderAsync(text, model, format, ct);
}
