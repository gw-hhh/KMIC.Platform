using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class RenameTicketCommandHandler(TicketRepository tickets, ProbeHooks hooks)
    : ICommandHandler<RenameTicketCommand, VersionedResponse<TicketId>>
{
    public async ValueTask<Result<VersionedResponse<TicketId>>> Handle(RenameTicketCommand command, CancellationToken cancellationToken)
    {
        var loaded = await tickets.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        loaded.Value.Rename(command.Title);
        await hooks.BeforeReturnAsync(cancellationToken);

        // 按当前值填写，提交后的版本由事务行为回填
        return new VersionedResponse<TicketId>(loaded.Value.Id, loaded.Value.ConcurrencyVersion);
    }
}
