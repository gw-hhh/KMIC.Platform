using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Retry;

/// <summary>
/// 处理 <see cref="RetryMessagesCommand"/>
/// </summary>
/// <remarks>
/// 写的是各模块自己的发件箱、收件箱表，不在 Operations 命令的事务里：这是框架存储的基础设施写入，和投递、处理程序改状态一样。
/// 每个状态条件都在 SQL 里，重复提交同一批不会改到别的行
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
            MessageActionOutcome.NotFound => new MessageReceipt(module, box, result.Id, MessageReceiptStatus.NotFound, "没有这一行"),
            _ => new MessageReceipt(module, box, result.Id, MessageReceiptStatus.WrongState, "这一行不是死信：已经处理完、正在处理，或者已经放弃了"),
        };
}
