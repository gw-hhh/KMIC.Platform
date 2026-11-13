using Kmic.Operations.Application.Messages;

namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 重试死信的请求
/// </summary>
/// <param name="Messages">要重试的消息，最多 200 条</param>
public sealed record RetryMessagesRequest(IReadOnlyList<MessageRef> Messages);
