namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 手写 SQL 的数据范围条件
/// </summary>
/// <remarks>
/// 查询带归属的表时必须用它；关系可见、资源可见性生成不了，要自己补
/// </remarks>
public interface IDataScopeSql
{
    /// <summary>
    /// 生成当前请求在某个范围对象上的条件，放进 WHERE 里
    /// </summary>
    /// <param name="ownerColumn">所有者列，如 <c>s."OwnerId"</c>；表里没有所有者就传空</param>
    /// <param name="orgUnitColumn">所属组织列，如 <c>s."OrgUnitId"</c>；表里没有所属组织就传空</param>
    /// <exception cref="ArgumentException">列写法不是“列”或“别名.列”（可以带双引号）</exception>
    FormattableString Condition(string scopeObject, string? ownerColumn, string? orgUnitColumn);
}
