using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

// 直接委托框架路由管理接口，不纳入 Operations 本地事务
public sealed class RedeliverCommandHandler(IMessageRouteAdmin routes) : ICommandHandler<RedeliverCommand, RedeliverResponse>
{
    public async ValueTask<Result<RedeliverResponse>> Handle(RedeliverCommand command, CancellationToken cancellationToken)
    {
        var result = await routes.RedeliverAsync(command.SourceModule, command.MessageType, command.Subscription, command.FromSeq, command.ToSeq, cancellationToken);
        return result.Outcome switch
        {
            RouteAdminOutcome.Done => new RedeliverResponse(result.Candidates, result.Written),
            RouteAdminOutcome.NotFound => OperationsErrors.RouteNotFound,
            RouteAdminOutcome.WrongStatus => OperationsErrors.RouteFinalizedForRedelivery,
            RouteAdminOutcome.UnknownSourceModule => OperationsErrors.RedeliverSourceUnknown,
            _ => OperationsErrors.RedeliverTooMany(IMessageRouteAdmin.MaxRedeliverRows),
        };
    }
}
