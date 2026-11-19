using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 查询死信或已放弃消息的详情
/// </summary>
public sealed class GetDeadMessageQueryHandler(IModuleMessageStores stores, IntegrationMessageCatalog messages) : IQueryHandler<GetDeadMessageQuery, DeadMessageDetailItem>
{
    /// <inheritdoc />
    public async ValueTask<Result<DeadMessageDetailItem>> Handle(GetDeadMessageQuery query, CancellationToken cancellationToken)
    {
        if (!MessageBoxKind.TryFromCode(query.Box, out var kind))
        {
            return CommonErrors.ValidationFailedWith([new FieldError("box", "Invalid", "只能是 Outbox 或 Inbox")]);
        }

        var detail = stores.Find(query.Module) is { } store ? await store.GetAsync(MessageItems.ToBox(kind), query.Id, cancellationToken) : null;
        // 此接口只查询死信和已放弃消息，其他状态按不存在处理
        if (detail is null || detail.State == DeadMessageState.Other)
        {
            return OperationsErrors.MessageNotFound;
        }

        return new DeadMessageDetailItem(
            MessageItems.ToItem(detail.Message),
            detail.State == DeadMessageState.Abandoned ? MessageState.Abandoned : MessageState.Dead,
            messages.CanShowPayload(detail.Message.MessageType, detail.Message.SchemaVersion) ? MessageItems.ElementOf(detail.Payload) : null,
            MessageItems.ElementOf(detail.Headers));
    }
}
