using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Abandon;

/// <summary>
/// 检查并放弃收件箱死信
/// </summary>
/// <remarks>
/// 不在 Operations 的命令事务里
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
                    receipts[(group.Key, id)] = Receipt(group.Key, id, MessageReceiptStatus.NotFound, "消息不存在");
                }
                else if (detail.State != DeadMessageState.Dead)
                {
                    receipts[(group.Key, id)] = Receipt(group.Key, id, MessageReceiptStatus.WrongState, detail.State == DeadMessageState.Abandoned ? "消息已放弃" : "消息当前不是死信，无法放弃");
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
                    MessageActionOutcome.NotFound => Receipt(group.Key, result.Id, MessageReceiptStatus.NotFound, "消息不存在"),

                    _ => Receipt(group.Key, result.Id, MessageReceiptStatus.WrongState, "消息状态已改变，无法放弃"),
                };
            }
        }

        var ordered = command.Messages.Select(m => receipts[(m.Module, m.Id)]).ToList();
        return new MessageReceipts(ordered);
    }

    private static MessageReceipt Receipt(string module, Guid id, MessageReceiptStatus status, string? reason) =>
        new(module, MessageBoxKind.Inbox, id, status, reason);
}
