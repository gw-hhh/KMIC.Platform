namespace Spike16.Tests;

internal sealed record Snapshot(Guid Id, long Version, string Name)
{
    public static Snapshot From(object?[] row) => new((Guid)row[0]!, (long)row[1]!, (string)row[2]!);
}
