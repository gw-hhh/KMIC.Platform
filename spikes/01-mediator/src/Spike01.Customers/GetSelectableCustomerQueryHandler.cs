using Spike01.BuildingBlocks;

namespace Spike01.Customers;

// 处理器 internal sealed（05-02 写法约定）
public sealed class GetSelectableCustomerQueryHandler(PipelineTrace trace, IExecutionContext context)
    : IQueryHandler<GetSelectableCustomerQuery, SelectableCustomer>
{
    public ValueTask<Result<SelectableCustomer>> Handle(GetSelectableCustomerQuery query, CancellationToken cancellationToken)
    {
        trace.Add($"Handler:GetSelectableCustomerQuery:{context.Origin}");
        Result<SelectableCustomer> result = new SelectableCustomer(query.CustomerId, "示例客户");
        return ValueTask.FromResult(result);
    }
}
