using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

public sealed class ChangePasswordCommandHandler(HandlerProbe probe, IExecutionContext context) : ICommandHandler<ChangePasswordCommand>
{
    public ValueTask<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        probe.Record(nameof(ChangePasswordCommandHandler), context.Origin);
        return ValueTask.FromResult(Result.Success());
    }
}
