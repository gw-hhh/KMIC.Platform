using System.Runtime.InteropServices;

namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// UTC 半开区间 [Start, EndExclusive)，用于按日期筛选时刻列
/// </summary>
/// <param name="Start">起始时刻（含）</param>
/// <param name="EndExclusive">结束时刻（不含）</param>
[StructLayout(LayoutKind.Auto)]
public readonly record struct UtcRange(DateTimeOffset Start, DateTimeOffset EndExclusive)
{
    /// <summary>
    /// 时刻是否落在区间内
    /// </summary>
    public bool Contains(DateTimeOffset instant) => instant >= Start && instant < EndExclusive;
}
