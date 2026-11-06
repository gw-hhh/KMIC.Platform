using Spike01.BuildingBlocks;

namespace Spike01.Customers;

/// <summary>
/// 客户模块对外的 Contracts 查询（05-07 第 4.1 节）
/// </summary>
public interface ICustomerSelectionQueries
{
    Task<Result<SelectableCustomer>> GetSelectableAsync(Guid customerId, CancellationToken cancellationToken);
}
