using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Retry;

/// <summary>
/// 重试死信，单条或批量
/// </summary>
/// <remarks>
/// 将死信重置为待处理并清零尝试次数，之后可被领取。逐条返回结果，非死信或不存在的记录不影响其他消息。
/// 重试不保证处理成功，原问题未解决时仍可能再次进入死信
/// </remarks>
/// <param name="Messages">要重试的消息，最多 200 条</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record RetryMessagesCommand(IReadOnlyList<MessageRef> Messages) : ICommand<MessageReceipts>;
