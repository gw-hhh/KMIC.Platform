using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Retry;

/// <summary>
/// 重试死信，单条或批量
/// </summary>
/// <param name="Messages">要重试的消息</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record RetryMessagesCommand(IReadOnlyList<MessageRef> Messages) : ICommand<MessageReceipts>;
