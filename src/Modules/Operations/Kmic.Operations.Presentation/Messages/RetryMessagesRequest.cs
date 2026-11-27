using Kmic.Operations.Application.Messages;

namespace Kmic.Operations.Presentation.Messages;

/// <param name="Messages">待重试的消息列表</param>
public sealed record RetryMessagesRequest(IReadOnlyList<MessageRef> Messages);
