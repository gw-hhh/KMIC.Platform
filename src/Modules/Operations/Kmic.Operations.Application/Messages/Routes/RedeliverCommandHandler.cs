using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 按序号范围重新投递消息
/// </summary>
/// <remarks>
/// 不在 Operations 的命令事务里
/// </remarks>
public sealed class RedeliverCommandHandler(IMessageRouteAdmin routes) : ICommandHandler<RedeliverCommand, RedeliverResponse>
{
    /// <inheritdoc />
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
