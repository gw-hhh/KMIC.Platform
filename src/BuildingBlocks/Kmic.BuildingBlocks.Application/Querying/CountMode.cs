namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 列表总数的计数方式
/// </summary>
/// <remarks>
/// 数据量会持续增长的列表用 <see cref="CappedAt"/>：最多数到上限加一行，超过上限时不再精确计数，
/// 也不能翻到上限之后的页（要缩小筛选范围或改用导出）
/// </remarks>
public readonly record struct CountMode
{
    /// <summary>
    /// 上限计数的默认上限
    /// </summary>
    public const int DefaultCap = 10_000;

    private CountMode(int cap) => Cap = cap;

    /// <summary>
    /// 精确计数，默认方式
    /// </summary>
    public static CountMode Exact => default;

    /// <summary>
    /// 上限，0 表示精确计数
    /// </summary>
    public int Cap { get; }

    /// <summary>
    /// 是否上限计数
    /// </summary>
    public bool IsCapped => Cap > 0;

    /// <summary>
    /// 上限计数
    /// </summary>
    /// <param name="cap">最多精确数到多少</param>
    public static CountMode CappedAt(int cap = DefaultCap)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(cap, 1);
        return new CountMode(cap);
    }
}
