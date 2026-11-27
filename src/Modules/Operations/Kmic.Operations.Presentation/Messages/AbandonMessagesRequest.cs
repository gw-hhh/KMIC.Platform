using Kmic.Operations.Application.Messages;

namespace Kmic.Operations.Presentation.Messages;

/// <param name="Messages">待放弃的消息列表</param>
/// <param name="Reason">放弃原因</param>
public sealed record AbandonMessagesRequest(IReadOnlyList<MessageRef> Messages, string Reason);
