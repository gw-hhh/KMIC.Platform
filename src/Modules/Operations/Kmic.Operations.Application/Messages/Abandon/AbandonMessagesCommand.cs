using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Abandon;

/// <summary>
/// 放弃收件箱死信，支持批量处理
/// </summary>
/// <remarks>
/// 发件箱死信只能重试
/// </remarks>
/// <param name="Messages">要放弃的消息</param>
/// <param name="Reason">原因</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record AbandonMessagesCommand(IReadOnlyList<MessageRef> Messages, string Reason) : ICommand<MessageReceipts>;
