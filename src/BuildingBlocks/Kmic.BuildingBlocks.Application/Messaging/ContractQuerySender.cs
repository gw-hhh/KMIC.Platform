using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

internal sealed class ContractQuerySender(IQuerySender sender) : IContractQuerySender
{
    public async ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct)
    {
        using var marker = ExecutionScope.EnterModuleCall();
        return await sender.SendAsync(query, ct);
    }
}
