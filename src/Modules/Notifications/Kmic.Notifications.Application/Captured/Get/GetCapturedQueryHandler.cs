using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kmic.Notifications.Application.Captured.Get;

/// <summary>
/// 处理 <see cref="GetCapturedQuery"/>
/// </summary>
public sealed class GetCapturedQueryHandler(INotificationsQueryDb db, IOptions<NotificationsCaptureOptions> capture) : IQueryHandler<GetCapturedQuery, CapturedDetailResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<CapturedDetailResponse>> Handle(GetCapturedQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!capture.Value.Enabled)
        {
            return NotificationsErrors.CapturedUnavailable;
        }

        var captured = await db.CapturedMessages
            .Where(c => c.Id == query.Id)
            .Select(c => new CapturedDetailResponse(c.Id, c.DeliveryId, c.Channel, c.Address, c.Role, c.Subject, c.Body, c.CapturedAt))
            .FirstOrDefaultAsync(cancellationToken);
        return captured is null ? NotificationsErrors.CapturedUnavailable : captured;
    }
}
