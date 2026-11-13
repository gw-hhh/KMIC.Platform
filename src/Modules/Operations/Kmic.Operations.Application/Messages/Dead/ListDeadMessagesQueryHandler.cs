using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 处理 <see cref="ListDeadMessagesQuery"/>
/// </summary>
public sealed class ListDeadMessagesQueryHandler(IModuleMessageStores stores, ICursorProtector protector)
    : IQueryHandler<ListDeadMessagesQuery, CursorPage<DeadMessageItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CursorPage<DeadMessageItem>>> Handle(ListDeadMessagesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        MessageBox? box = null;
        if (query.Box is { Length: > 0 } code)
        {
            if (!MessageBoxKind.TryFromCode(code, out var kind))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("box", "Invalid", "只能是 Outbox 或 Inbox")]);
            }

            box = MessageItems.ToBox(kind);
        }

        IEnumerable<IModuleMessageStore> selected = stores.All;
        if (query.Module is { Length: > 0 } module)
        {
            selected = stores.Find(module) is { } one ? [one] : [];
        }

        var filter = new DeadMessageFilter(box, query.Subscription is { Length: > 0 } ? query.Subscription : null, query.MessageType is { Length: > 0 } ? query.MessageType : null);
        var page = await MergedCursorPaging.PageAsync(
            selected,
            query.Cursor,
            protector,
            (store, after, take) => store.ListDeadAsync(filter, after is { } position ? new MessagePosition(position.At, position.Id) : null, take, cancellationToken),
            message => message.OccurredAt,
            message => message.Id);
        if (page.IsFailure)
        {
            return page.Error;
        }

        return new CursorPage<DeadMessageItem>([.. page.Value.Items.Select(MessageItems.ToItem)], page.Value.NextCursor);
    }
}
