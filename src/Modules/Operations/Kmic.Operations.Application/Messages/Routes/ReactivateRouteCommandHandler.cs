using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 处理 <see cref="ReactivateRouteCommand"/>
/// </summary>
public sealed class ReactivateRouteCommandHandler(IMessageRouteAdmin routes) : ICommandHandler<ReactivateRouteCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ReactivateRouteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        return await routes.ReactivateAsync(command.MessageType, command.Subscription, cancellationToken) switch
        {
            RouteAdminOutcome.Done => Result.Success(),
            RouteAdminOutcome.NotFound => OperationsErrors.RouteNotFound,
            _ => OperationsErrors.RouteNotFinalized,
        };
    }
}
