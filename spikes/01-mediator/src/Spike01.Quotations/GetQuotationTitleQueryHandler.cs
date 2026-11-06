using Spike01.BuildingBlocks;

namespace Spike01.Quotations;

public sealed class GetQuotationTitleQueryHandler(PipelineTrace trace) : IQueryHandler<GetQuotationTitleQuery, string>
{
    public ValueTask<Result<string>> Handle(GetQuotationTitleQuery query, CancellationToken cancellationToken)
    {
        trace.Add("Handler:GetQuotationTitleQuery");
        Result<string> result = "示例报价";
        return ValueTask.FromResult(result);
    }
}
