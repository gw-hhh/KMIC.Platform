namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal static class TicketNumbers
{
    private static int _last;

    public static string Next() => $"T-{Interlocked.Increment(ref _last):D6}";
}
