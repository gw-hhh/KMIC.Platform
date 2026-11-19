using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Abandon;

/// <summary>
/// 放弃收件箱死信，支持批量处理
/// </summary>
/// <remarks>
/// 要通过接收方的放弃检查，没通过的保持原状并在逐条结果里说明原因；发件箱死信只能重试
/// </remarks>
/// <param name="Messages">要放弃的消息，最多 200 条，都在收件箱</param>
/// <param name="Reason">原因，必填，最多 500 个字符</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record AbandonMessagesCommand(IReadOnlyList<MessageRef> Messages, string Reason) : ICommand<MessageReceipts>;
