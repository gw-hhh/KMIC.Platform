using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Rebuilds;

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
