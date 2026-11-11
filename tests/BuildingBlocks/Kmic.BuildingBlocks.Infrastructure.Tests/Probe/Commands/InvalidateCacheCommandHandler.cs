using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class InvalidateCacheCommandHandler(IAfterCommit afterCommit) : ICommandHandler<InvalidateCacheCommand>
{
    public ValueTask<Result> Handle(InvalidateCacheCommand command, CancellationToken cancellationToken)
    {
        foreach (var tag in command.Tags)
        {
            afterCommit.InvalidateTags(tag);
        }

        return ValueTask.FromResult<Result>(command.Fail ? TicketErrors.Rejected : Result.Success());
    }
}
