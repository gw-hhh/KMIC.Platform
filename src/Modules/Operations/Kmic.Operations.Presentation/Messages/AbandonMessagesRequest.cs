using Kmic.Operations.Application.Messages;

namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 放弃死信的请求
/// </summary>
/// <param name="Messages">要放弃的消息</param>
/// <param name="Reason">原因</param>
public sealed record AbandonMessagesRequest(IReadOnlyList<MessageRef> Messages, string Reason);
