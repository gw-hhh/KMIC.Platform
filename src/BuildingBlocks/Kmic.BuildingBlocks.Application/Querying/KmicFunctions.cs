namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 文本匹配函数
/// </summary>
/// <remarks>
/// 模糊查询都用这里的函数，不用 Contains、StartsWith、ToLower。不区分英文大小写，term 里的 % _ \ 按普通字符处理，只能在数据库查询里用
/// </remarks>
public static class KmicFunctions
{
    /// <summary>
    /// 包含
    /// </summary>
    /// <exception cref="InvalidOperationException">不在数据库查询里调用时抛出</exception>
    public static bool ContainsText(string? column, string term) => throw OutsideQuery();

    /// <summary>
    /// 开头是
    /// </summary>
    /// <exception cref="InvalidOperationException">不在数据库查询里调用时抛出</exception>
    public static bool StartsWithText(string? column, string term) => throw OutsideQuery();

    private static InvalidOperationException OutsideQuery() => new("KmicFunctions 只能在数据库查询中使用。");
}
