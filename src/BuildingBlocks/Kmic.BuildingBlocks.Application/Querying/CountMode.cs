namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>列表总数计数方式；持续增长的列表建议使用 <see cref="CappedAt"/>。</summary>
public readonly record struct CountMode
{
    /// <summary>上限计数的默认上限。</summary>
    public const int DefaultCap = 10_000;

    private CountMode(int cap) => Cap = cap;

    /// <summary>精确计数（默认）。</summary>
    public static CountMode Exact => default;

    /// <summary>上限，0 表示精确计数。</summary>
    public int Cap { get; }

    public bool IsCapped => Cap > 0;

    public static CountMode CappedAt(int cap = DefaultCap)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(cap, 1);
        return new CountMode(cap);
    }
}
