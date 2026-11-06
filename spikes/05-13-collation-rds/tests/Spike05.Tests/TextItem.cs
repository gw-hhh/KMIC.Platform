namespace Spike05.Tests;

public sealed class TextItem
{
    public int Id { get; set; }

    // 默认列：区分大小写
    public string Name { get; set; } = null!;

    // 不区分大小写的列（账号、编码）
    public string Code { get; set; } = null!;
}
