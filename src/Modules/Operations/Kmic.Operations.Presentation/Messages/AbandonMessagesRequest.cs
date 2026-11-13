using Kmic.Operations.Application.Messages;

namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 放弃死信的请求
/// </summary>
/// <param name="Messages">要放弃的消息，最多 200 条，都在收件箱</param>
/// <param name="Reason">原因，必填，最多 500 个字符</param>
public sealed record AbandonMessagesRequest(IReadOnlyList<MessageRef> Messages, string Reason);
