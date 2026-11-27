using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

// 直接委托框架路由管理接口，不纳入 Operations 本地事务
public sealed class FinalizeRouteCommandHandler(IMessageRouteAdmin routes, IExecutionContext execution)
    : ICommandHandler<FinalizeRouteCommand, FinalizeRouteResponse>
{
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
