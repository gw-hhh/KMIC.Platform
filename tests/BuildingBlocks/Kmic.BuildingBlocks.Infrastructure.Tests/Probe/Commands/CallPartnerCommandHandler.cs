using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class CallPartnerCommandHandler(IPartnerClient partner) : ICommandHandler<CallPartnerCommand>
{
    public async ValueTask<Result> Handle(CallPartnerCommand command, CancellationToken cancellationToken)
    {
        await partner.GetStatusAsync(cancellationToken);
        return Result.Success();
    }
}
