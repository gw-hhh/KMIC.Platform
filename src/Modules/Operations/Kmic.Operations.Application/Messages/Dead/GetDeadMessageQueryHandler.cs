using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 处理 <see cref="GetDeadMessageQuery"/>
/// </summary>
public sealed class GetDeadMessageQueryHandler(IModuleMessageStores stores) : IQueryHandler<GetDeadMessageQuery, DeadMessageDetailItem>
{
    /// <inheritdoc />
    public async ValueTask<Result<DeadMessageDetailItem>> Handle(GetDeadMessageQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!MessageBoxKind.TryFromCode(query.Box, out var kind))
        {
            return CommonErrors.ValidationFailedWith([new FieldError("box", "Invalid", "只能是 Outbox 或 Inbox")]);
        }

        var detail = stores.Find(query.Module) is { } store ? await store.GetAsync(MessageItems.ToBox(kind), query.Id, cancellationToken) : null;
        // 不是死信也不是放弃的（待处理、处理中、已处理）按没有这条死信处理：这里不是看任意消息的入口
        if (detail is null || detail.State == DeadMessageState.Other)
        {
            return OperationsErrors.MessageNotFound;
        }

        return new DeadMessageDetailItem(
            MessageItems.ToItem(detail.Message),
            detail.State == DeadMessageState.Abandoned ? MessageState.Abandoned : MessageState.Dead,
            MessageItems.ElementOf(detail.Payload),
            MessageItems.ElementOf(detail.Headers));
    }
}
