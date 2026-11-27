using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Processing;

/// <summary>单次请求渲染出的各渠道内容（与收件人无关）</summary>
/// <param name="Channels">各渠道渲染内容</param>
/// <param name="CodeHash">模板声明与默认内容哈希</param>
public sealed record RenderedContent(IReadOnlyDictionary<TemplateChannel, ChannelContent> Channels, string CodeHash)
{
    public ChannelContent For(TemplateChannel channel) => Channels[channel];
}
