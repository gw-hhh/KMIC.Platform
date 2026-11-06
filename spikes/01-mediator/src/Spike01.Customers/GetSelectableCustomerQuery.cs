using Spike01.BuildingBlocks;

namespace Spike01.Customers;

/// <summary>
/// 按调用方数据范围判断客户能否选用
/// </summary>
[ContractQuery(ContractAccess.CallerScoped)]
public sealed record GetSelectableCustomerQuery(Guid CustomerId) : IQuery<SelectableCustomer>;
