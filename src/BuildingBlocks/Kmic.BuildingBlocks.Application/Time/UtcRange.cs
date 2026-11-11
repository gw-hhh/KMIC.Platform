using System.Runtime.InteropServices;

namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// UTC 时间区间
/// </summary>
/// <param name="Start">开始时间（含）</param>
/// <param name="EndExclusive">结束时间（不含）</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct UtcRange(DateTimeOffset Start, DateTimeOffset EndExclusive)
{
    /// <summary>
    /// 是否在区间内
    /// </summary>
    public bool Contains(DateTimeOffset instant) => instant >= Start && instant < EndExclusive;
}
