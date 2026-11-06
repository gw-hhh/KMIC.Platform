namespace Spike16.Tests;

internal sealed record Row(long SourceVersion, string Name, bool IsTerminal, short? TerminalReason, Guid? RebuildRunId)
{
    public static Row From(object?[] row) => new((long)row[0]!, (string)row[1]!, (bool)row[2]!, (short?)row[3], (Guid?)row[4]);
}
