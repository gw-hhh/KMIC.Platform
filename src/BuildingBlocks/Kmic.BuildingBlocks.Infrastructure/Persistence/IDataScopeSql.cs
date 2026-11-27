namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>手写 SQL 数据范围条件生成器（关系/资源级可见性需由业务 SQL 自行补充）</summary>
public interface IDataScopeSql
{
    /// <summary>生成当前请求针对指定范围对象的 SQL 过滤条件（用于拼接至 WHERE 子句）</summary>
    /// <param name="ownerColumn">所有者列名，格式为“列”或“别名.列”（可带引号），无该列传 null</param>
    /// <param name="orgUnitColumn">所属组织列名，格式同上，无该列传 null</param>
    FormattableString Condition(string scopeObject, string? ownerColumn, string? orgUnitColumn);
}
