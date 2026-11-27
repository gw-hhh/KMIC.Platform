namespace Kmic.Customer.Application;

/// <summary>Customer 只读查询入口（默认过滤软删除与数据范围，不含未提交变更），在此补充 IQueryable 属性</summary>
public interface ICustomerQueryDb;
