using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Abandon;

/// <summary>
/// 放弃收件箱里的死信，单条或批量
/// </summary>
/// <remarks>
/// 放弃意味着这些消息永远不会被处理，所以原因必填。放弃之前先调接收方为这种消息注册的放弃检查：
/// 检查不让放弃的（比如 Files 的 AttachFiles，引用的文件还没有归属）不改状态，回执里写明原因，只能重试。
/// 只有收件箱的死信能放弃，发件箱的死信是投递本身出了问题，只能重试
/// </remarks>
/// <param name="Messages">要放弃的消息，最多 200 条，都在收件箱</param>
/// <param name="Reason">原因，必填，最多 500 个字符</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record AbandonMessagesCommand(IReadOnlyList<MessageRef> Messages, string Reason) : ICommand<MessageReceipts>;
