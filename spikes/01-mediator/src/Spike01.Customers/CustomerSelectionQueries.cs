using Spike01.BuildingBlocks;

namespace Spike01.Customers;

// 薄适配器：把参数转为本模块的查询，经 IContractQuerySender 发送（05-07 D2）
internal sealed class CustomerSelectionQueries(IContractQuerySender sender) : ICustomerSelectionQueries
{
    public Task<Result<SelectableCustomer>> GetSelectableAsync(Guid customerId, CancellationToken cancellationToken) =>
        sender.SendAsync(new GetSelectableCustomerQuery(customerId), cancellationToken);
}
