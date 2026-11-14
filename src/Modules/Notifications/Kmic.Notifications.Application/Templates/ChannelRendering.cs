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
    // strict 为真时，模板里用到数据模型没有的变量也算有误（发布和预览时用）
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
