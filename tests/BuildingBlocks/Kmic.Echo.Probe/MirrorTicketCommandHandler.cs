using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Echo.Probe;

public sealed class MirrorTicketCommandHandler(IIntegrationEventPublisher publisher, IReadModelWriter<TicketSummary> summaries, EchoHooks hooks)
    : ICommandHandler<MirrorTicketCommand>
{
    public async ValueTask<Result> Handle(MirrorTicketCommand command, CancellationToken cancellationToken)
    {
        if (hooks.Throws > 0)
        {
            hooks.Throws--;
            throw new InvalidOperationException("模拟处理器出错");
        }

        if (hooks.Failures.Remove(command.Title, out var failure))
        {
            return failure;
        }

        await summaries.UpsertAsync(
            new TicketSummary { Id = command.TicketId, SourceVersion = command.SourceVersion, Status = ReferenceStatus.Active, Title = command.Title },
            cancellationToken);
        publisher.Add(new TicketMirrored(command.TicketId, command.Title, command.SourceVersion));
        if (hooks.BeforeReturn is { } beforeReturn)
        {
            await beforeReturn(cancellationToken);
        }

        return Result.Success();
    }
}
