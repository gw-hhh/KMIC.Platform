using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 一次发送请求渲染出的内容：与收件人无关，每个渠道一份
/// </summary>
/// <param name="Channels">模板声明的各个渠道的内容</param>
/// <param name="CodeHash">发送时的模板声明及默认内容哈希</param>
public sealed record RenderedContent(IReadOnlyDictionary<TemplateChannel, ChannelContent> Channels, string CodeHash)
{
    /// <summary>
    /// 某个渠道的内容
    /// </summary>
    /// <param name="channel">渠道，必须是模板声明过的</param>
    public ChannelContent For(TemplateChannel channel) => Channels[channel];
}
