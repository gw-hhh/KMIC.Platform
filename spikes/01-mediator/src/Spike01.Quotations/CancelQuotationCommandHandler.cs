using Spike01.BuildingBlocks;

namespace Spike01.Quotations;

public sealed class CancelQuotationCommandHandler(PipelineTrace trace) : ICommandHandler<CancelQuotationCommand>
{
    public ValueTask<Result> Handle(CancelQuotationCommand command, CancellationToken cancellationToken)
    {
        trace.Add("Handler:CancelQuotationCommand");
        return ValueTask.FromResult(Result.Success());
    }
}
