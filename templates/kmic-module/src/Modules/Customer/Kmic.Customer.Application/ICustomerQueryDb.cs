namespace Kmic.Customer.Application;

/// <summary>
/// Customer 模块的查询入口
/// </summary>
/// <remarks>
/// 查询已应用数据范围和软删除过滤，不返回命令中尚未保存的修改。
/// 为可查询的聚合添加 IQueryable 属性
/// </remarks>
public interface ICustomerQueryDb;
