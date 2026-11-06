using Spike01.BuildingBlocks;
using Spike01.Customers;

namespace Spike01.Quotations;

// 命令处理器中跨模块调用客户模块的 Contracts 查询
public sealed class CreateQuotationCommandHandler(
    ICustomerSelectionQueries customers,
    PipelineTrace trace,
    IExecutionContext context,
    TimeProvider timeProvider)
    : ICommandHandler<CreateQuotationCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateQuotationCommand command, CancellationToken cancellationToken)
    {
        trace.Add($"Handler:CreateQuotationCommand:before:{context.Origin}");
        var customer = await customers.GetSelectableAsync(command.CustomerId, cancellationToken);
        trace.Add($"Handler:CreateQuotationCommand:after:{context.Origin}");
        if (!customer.IsSuccess)
        {
            return customer.Error!;
        }

        return Guid.CreateVersion7(timeProvider.GetUtcNow());
    }
}
