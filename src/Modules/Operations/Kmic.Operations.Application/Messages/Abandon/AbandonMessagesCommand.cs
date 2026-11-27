using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Abandon;

/// <summary>批量放弃收件箱死信（发件箱死信仅支持重试）。</summary>
/// <param name="Messages">要放弃的消息</param>
/// <param name="Reason">原因</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record AbandonMessagesCommand(IReadOnlyList<MessageRef> Messages, string Reason) : ICommand<MessageReceipts>;
