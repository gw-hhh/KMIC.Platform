using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Customer.Application;

/// <summary>在此声明存在数据归属的聚合常量（格式为“Customer.资源”），并标注 [DataScopeObject("显示名")]</summary>
[DataScopeObjectGroup("Customer")]
public static class CustomerScopes
{
}
