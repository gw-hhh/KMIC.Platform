using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Retry;

/// <summary>
/// 重试死信，单条或批量
/// </summary>
/// <remarks>
/// 状态回到待处理，次数清零，马上可以被领取。每条各有各的回执：已经不是死信的、不存在的不影响别的。
/// 重试不保证成功：问题没解决的消息会再次失败，再次成为死信
/// </remarks>
/// <param name="Messages">要重试的消息，最多 200 条</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record RetryMessagesCommand(IReadOnlyList<MessageRef> Messages) : ICommand<MessageReceipts>;
