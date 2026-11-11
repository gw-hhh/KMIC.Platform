namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询中的文本匹配函数；所有模糊匹配都用它们，不用 Contains、StartsWith、ToLower
/// </summary>
/// <remarks>
/// 不区分 ASCII 字母大小写，中文不受影响；term 中的 % _ \ 按普通字符处理。只能在数据库查询中使用，
/// 翻译方式以后可以调整（如配合 trigram 索引），调用方写法不变
/// </remarks>
public static class KmicFunctions
{
    /// <summary>
    /// 列中包含 term
    /// </summary>
    /// <exception cref="InvalidOperationException">在查询之外调用</exception>
    public static bool ContainsText(string? column, string term) => throw OutsideQuery();

    /// <summary>
    /// 列以 term 开头，如按编码前缀查找
    /// </summary>
    /// <exception cref="InvalidOperationException">在查询之外调用</exception>
    public static bool StartsWithText(string? column, string term) => throw OutsideQuery();

    private static InvalidOperationException OutsideQuery() => new("KmicFunctions 只能在数据库查询中使用。");
}
