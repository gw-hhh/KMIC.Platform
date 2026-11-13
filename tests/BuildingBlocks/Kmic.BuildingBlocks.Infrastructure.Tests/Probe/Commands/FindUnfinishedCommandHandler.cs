using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class FindUnfinishedCommandHandler(IUnfinishedMessageProbe probe) : ICommandHandler<FindUnfinishedCommand, LockedThings>
{
    public async ValueTask<Result<LockedThings>> Handle(FindUnfinishedCommand command, CancellationToken cancellationToken) =>
        new LockedThings([.. await probe.FindSubjectsAsync(command.MessageType, command.Ids, cancellationToken)]);
}
