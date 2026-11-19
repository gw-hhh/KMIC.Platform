using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Retry;

/// <summary>
/// 重试死信，单条或批量
/// </summary>
/// <remarks>
/// 重置为待处理并清零尝试次数；逐条返回结果，非死信或不存在的不影响其他消息。原问题没解决时还会再进死信
/// </remarks>
/// <param name="Messages">要重试的消息，最多 200 条</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record RetryMessagesCommand(IReadOnlyList<MessageRef> Messages) : ICommand<MessageReceipts>;
