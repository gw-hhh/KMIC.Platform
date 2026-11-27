namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>数据库查询表达式专用的文本匹配函数（不区分英文大小写），在查询表达式之外调用将抛出异常。</summary>
public static class KmicFunctions
{
    /// <summary>包含文本匹配。</summary>
    public static bool ContainsText(string? column, string term) => throw OutsideQuery();

    /// <summary>前缀文本匹配。</summary>
    public static bool StartsWithText(string? column, string term) => throw OutsideQuery();

    private static InvalidOperationException OutsideQuery() => new("KmicFunctions 只能在数据库查询中使用。");
}
