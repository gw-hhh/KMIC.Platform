using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 把框架的重建运维结果换成 Operations 的错误
/// </summary>
internal static class RebuildOutcomes
{
    public static Result Of(RebuildAdminOutcome outcome) =>
        outcome switch
        {
            RebuildAdminOutcome.Done => Result.Success(),
            RebuildAdminOutcome.UnknownSubscription => OperationsErrors.RebuildSubscriptionUnknown,
            RebuildAdminOutcome.AlreadyRunning => OperationsErrors.RebuildAlreadyRunning,
            _ => OperationsErrors.RebuildNotAwaitingConfirmation,
        };
}
