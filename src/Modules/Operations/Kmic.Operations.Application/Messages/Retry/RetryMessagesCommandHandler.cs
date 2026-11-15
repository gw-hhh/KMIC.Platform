using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Retry;

/// <summary>
/// 重试死信并逐条返回结果
/// </summary>
/// <remarks>
/// 直接更新各模块的消息存储，不参与 Operations 命令事务
/// </remarks>
public sealed class RetryMessagesCommandHandler(IModuleMessageStores stores) : ICommandHandler<RetryMessagesCommand, MessageReceipts>
{
    /// <inheritdoc />
    public async ValueTask<Result<MessageReceipts>> Handle(RetryMessagesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var outcomes = new Dictionary<(string Module, MessageBoxKind Box, Guid Id), MessageReceipt>();
        foreach (var group in command.Messages.GroupBy(m => (m.Module, m.Box)))
        {
            var ids = group.Select(m => m.Id).Distinct().ToList();
            var store = stores.Find(group.Key.Module);
            var results = store is null
                ? ids.Select(id => new MessageActionResult(id, MessageActionOutcome.NotFound)).ToList()
                : await store.RetryAsync(MessageItems.ToBox(group.Key.Box), ids, cancellationToken);
            foreach (var result in results)
            {
                outcomes[(group.Key.Module, group.Key.Box, result.Id)] = Receipt(group.Key.Module, group.Key.Box, result);
            }
        }

        var receipts = command.Messages.Select(m => outcomes[(m.Module, m.Box, m.Id)]).ToList();
        return new MessageReceipts(receipts);
    }

    private static MessageReceipt Receipt(string module, MessageBoxKind box, MessageActionResult result) =>
        result.Outcome switch
        {
            MessageActionOutcome.Done => new MessageReceipt(module, box, result.Id, MessageReceiptStatus.Done, null),
            MessageActionOutcome.NotFound => new MessageReceipt(module, box, result.Id, MessageReceiptStatus.NotFound, "消息不存在"),
            _ => new MessageReceipt(module, box, result.Id, MessageReceiptStatus.WrongState, "消息当前不是死信，无法重试"),
        };
}
