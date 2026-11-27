using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

public sealed class ReactivateRouteCommandHandler(IMessageRouteAdmin routes) : ICommandHandler<ReactivateRouteCommand>
{
    public async ValueTask<Result> Handle(ReactivateRouteCommand command, CancellationToken cancellationToken)
    {
        return await routes.ReactivateAsync(command.MessageType, command.Subscription, cancellationToken) switch
        {
            RouteAdminOutcome.Done => Result.Success(),
            RouteAdminOutcome.NotFound => OperationsErrors.RouteNotFound,
            _ => OperationsErrors.RouteNotFinalized,
        };
    }
}
