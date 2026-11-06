namespace Spike01.BuildingBlocks;

internal sealed class ContractQuerySender(IQuerySender sender) : IContractQuerySender
{
    public async Task<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken)
    {
        var previous = ExecutionContext.Override;
        ExecutionContext.Override = CallOrigin.ModuleCall;
        try
        {
            return await sender.SendAsync(query, cancellationToken);
        }
        finally
        {
            ExecutionContext.Override = previous;
        }
    }
}
