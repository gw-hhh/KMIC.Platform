namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 手写 SQL 的数据范围条件
/// </summary>
/// <remarks>
/// 手写 SQL 绕过了 EF 的查询过滤器，查询带归属的表时必须用它生成条件，和过滤器取同一个范围、得到同样的结果。
/// 只能写在 Infrastructure（Application 不能用手写 SQL）。关系可见、资源可见性生成不了，要自己补全并用测试覆盖。
/// 返回的是带参数的 <see cref="FormattableString"/>，和查询的其余部分用 <see cref="SqlFragments.Concat"/> 拼起来，交给 <c>SqlQuery</c>、<c>FromSql</c> 执行
/// </remarks>
public interface IDataScopeSql
{
    /// <summary>
    /// 生成当前请求在某个范围对象上的条件，放进 WHERE 里用
    /// </summary>
    /// <remarks>
    /// 不受限（系统身份、范围等级是全部）时条件是 <c>TRUE</c>；没有任何可见范围（匿名、没有所有者也没有组织）时是 <c>FALSE</c>。
    /// 范围值都是参数，不拼进 SQL 文本
    /// </remarks>
    /// <param name="scopeObject">范围对象编码，和聚合声明数据范围时用的一致</param>
    /// <param name="ownerColumn">所有者列，如 <c>s."OwnerId"</c>；表里没有所有者就传空</param>
    /// <param name="orgUnitColumn">所属组织列，如 <c>s."OrgUnitId"</c>；表里没有所属组织就传空</param>
    /// <exception cref="ArgumentException">列写法不是“列”或“别名.列”（可以带双引号）</exception>
    FormattableString Condition(string scopeObject, string? ownerColumn, string? orgUnitColumn);
}
