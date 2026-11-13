using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>
/// 处理 <see cref="ListCapturedQuery"/>
/// </summary>
public sealed class ListCapturedQueryHandler(INotificationsQueryDb db, IOptions<NotificationsCaptureOptions> capture)
    : IQueryHandler<ListCapturedQuery, PagedResult<CapturedListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<CapturedListItem>>> Handle(ListCapturedQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!capture.Value.Enabled)
        {
            return NotificationsErrors.CapturedUnavailable;
        }

        var captured = db.CapturedMessages;
        if (query.Channel is { } code)
        {
            var channel = DeliveryChannel.FromCode(code);
            captured = captured.Where(c => c.Channel == channel);
        }

        return await captured
            .Select(c => new CapturedListItem(c.Id, c.DeliveryId, c.Channel, c.Address, c.Role, c.Subject, c.CapturedAt))
            .ToPagedAsync(query.Page, query.Sort, CapturedSort.Map, cancellationToken);
    }
}
