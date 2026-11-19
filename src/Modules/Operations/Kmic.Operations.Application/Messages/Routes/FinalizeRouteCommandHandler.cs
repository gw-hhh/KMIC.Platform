using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 终结消息订阅路由
/// </summary>
/// <remarks>
/// 不在 Operations 的命令事务里
/// </remarks>
public sealed class FinalizeRouteCommandHandler(IMessageRouteAdmin routes, IExecutionContext execution)
    : ICommandHandler<FinalizeRouteCommand, FinalizeRouteResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<FinalizeRouteResponse>> Handle(FinalizeRouteCommand command, CancellationToken cancellationToken)
    {
        var user = execution.User;
        var by = user.ActualUserId ?? user.UserId ?? UserId.System;
        var result = await routes.FinalizeAsync(
            command.MessageType, command.Subscription, by, command.Reason.Trim(), command.AbandonUnfinished, InstanceSubscriptions.AliveWithin, cancellationToken);
        return result.Outcome switch
        {
            RouteAdminOutcome.Done => new FinalizeRouteResponse(result.Unfinished),
            RouteAdminOutcome.NotFound => OperationsErrors.RouteNotFound,
            RouteAdminOutcome.WrongStatus => OperationsErrors.RouteNotRetiring,
            RouteAdminOutcome.StillDeclared => OperationsErrors.RouteStillDeclared,
            RouteAdminOutcome.InFlight => OperationsErrors.RouteInFlight,
            _ => OperationsErrors.RouteHasUnfinished(result.Unfinished),
        };
    }
}
