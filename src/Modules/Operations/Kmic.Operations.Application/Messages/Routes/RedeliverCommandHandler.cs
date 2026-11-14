using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 处理 <see cref="RedeliverCommand"/>
/// </summary>
/// <remarks>
/// 写的是订阅方的收件箱，由框架在自己的事务里完成：这是框架存储的基础设施写入，和正常投递一样
/// </remarks>
public sealed class RedeliverCommandHandler(IMessageRouteAdmin routes) : ICommandHandler<RedeliverCommand, RedeliverResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<RedeliverResponse>> Handle(RedeliverCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

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
