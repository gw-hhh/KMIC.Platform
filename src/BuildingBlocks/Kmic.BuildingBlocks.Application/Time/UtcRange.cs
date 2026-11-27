using System.Runtime.InteropServices;

namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>半开 UTC 时间区间 [Start, EndExclusive)。</summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct UtcRange(DateTimeOffset Start, DateTimeOffset EndExclusive)
{
    public bool Contains(DateTimeOffset instant) => instant >= Start && instant < EndExclusive;
}
