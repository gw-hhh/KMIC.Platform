using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Maintenance;

/// <summary>
/// 清理过期的登录失败记录
/// </summary>
public sealed class PurgeLoginThrottleCommandHandler(ILoginThrottleStore throttles, IOptions<LoginThrottleOptions> options, TimeProvider time)
    : ICommandHandler<PurgeLoginThrottleCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeLoginThrottleCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var purged = await throttles.PurgeAsync(now - options.Value.ResetAfter, now, cancellationToken);
        return new ScheduledJobOutcome(purged);
    }
}
