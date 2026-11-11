using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class OpenTicketCommandHandler(TicketRepository tickets, IExecutionContext execution, ProbeHooks hooks)
    : ICommandHandler<OpenTicketCommand, CreatedResponse<TicketId>>
{
    public async ValueTask<Result<CreatedResponse<TicketId>>> Handle(OpenTicketCommand command, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("测试命令要求用户身份。");
        var orgUnit = access.OrgUnitId ?? throw new InvalidOperationException("测试身份要有所属组织。");
        var ticket = Ticket.Create(command.Number, access.UserId, orgUnit, command.Title);
        tickets.Add(ticket);
        hooks.Handled++;
        await hooks.BeforeReturnAsync(cancellationToken);

        return string.Equals(command.Title, OpenTicketCommand.RejectedTitle, StringComparison.Ordinal) ? TicketErrors.Rejected : new CreatedResponse<TicketId>(ticket.Id);
    }
}
