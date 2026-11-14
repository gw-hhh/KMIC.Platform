using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 处理 <see cref="FinalizeRouteCommand"/>
/// </summary>
/// <remarks>
/// 写的是路由表和订阅方的收件箱，由框架在同一个事务里完成，不在 Operations 命令的事务里：
/// 这是框架存储的基础设施写入，和重试、放弃死信一样
/// </remarks>
public sealed class FinalizeRouteCommandHandler(IMessageRouteAdmin routes, IExecutionContext execution)
    : ICommandHandler<FinalizeRouteCommand, FinalizeRouteResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<FinalizeRouteResponse>> Handle(FinalizeRouteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

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
