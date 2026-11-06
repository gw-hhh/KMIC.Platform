namespace Spike05.Tests;

// 06 第 11.2 节：所有模糊匹配都用这两个函数，只能在查询中使用
public static class KmicFunctions
{
    // 不区分 ASCII 大小写的“包含”匹配；term 中的 % _ \ 按普通字符处理
    public static bool ContainsText(string? column, string term) =>
        throw new InvalidOperationException("KmicFunctions can only be used in database queries.");

    public static bool StartsWithText(string? column, string term) =>
        throw new InvalidOperationException("KmicFunctions can only be used in database queries.");
}
