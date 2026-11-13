using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Abandon;

/// <summary>
/// 处理 <see cref="AbandonMessagesCommand"/>
/// </summary>
/// <remarks>
/// 先逐条取回内容、问接收方的放弃检查，通过的再一起改状态；检查不让放弃的、不存在的、不是死信的，各有各的回执，不影响别的。
/// 写的是各模块自己的收件箱表，不在 Operations 命令的事务里（框架存储的基础设施写入）
/// </remarks>
public sealed class AbandonMessagesCommandHandler(IModuleMessageStores stores, IAbandonGuard guard) : ICommandHandler<AbandonMessagesCommand, MessageReceipts>
{
    /// <inheritdoc />
    public async ValueTask<Result<MessageReceipts>> Handle(AbandonMessagesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var reason = command.Reason.Trim();
        var receipts = new Dictionary<(string Module, Guid Id), MessageReceipt>();
        foreach (var group in command.Messages.GroupBy(m => m.Module, StringComparer.Ordinal))
        {
            var store = stores.Find(group.Key);
            var allowed = new List<Guid>();
            foreach (var id in group.Select(m => m.Id).Distinct())
            {
                var detail = store is null ? null : await store.GetAsync(MessageBox.Inbox, id, cancellationToken);
                if (detail is null)
                {
                    receipts[(group.Key, id)] = Receipt(group.Key, id, MessageReceiptStatus.NotFound, "没有这一行");
                }
                else if (detail.State != DeadMessageState.Dead)
                {
                    receipts[(group.Key, id)] = Receipt(group.Key, id, MessageReceiptStatus.WrongState, detail.State == DeadMessageState.Abandoned ? "这一行已经放弃了" : "这一行不是死信：已经处理完，或者正在处理");
                }
                else if (await guard.CheckAsync(detail, cancellationToken) is { IsFailure: true } refused)
                {
                    receipts[(group.Key, id)] = Receipt(group.Key, id, MessageReceiptStatus.Refused, refused.Error.Message);
                }
                else
                {
                    allowed.Add(id);
                }
            }

            if (store is null || allowed.Count == 0)
            {
                continue;
            }

            foreach (var result in await store.AbandonAsync(allowed, reason, cancellationToken))
            {
                receipts[(group.Key, result.Id)] = result.Outcome switch
                {
                    MessageActionOutcome.Done => Receipt(group.Key, result.Id, MessageReceiptStatus.Done, null),
                    MessageActionOutcome.NotFound => Receipt(group.Key, result.Id, MessageReceiptStatus.NotFound, "没有这一行"),

                    // 检查通过之后、改状态之前被别人处理了
                    _ => Receipt(group.Key, result.Id, MessageReceiptStatus.WrongState, "这一行已经不是死信了"),
                };
            }
        }

        var ordered = command.Messages.Select(m => receipts[(m.Module, m.Id)]).ToList();
        return new MessageReceipts(ordered);
    }

    private static MessageReceipt Receipt(string module, Guid id, MessageReceiptStatus status, string? reason) =>
        new(module, MessageBoxKind.Inbox, id, status, reason);
}
