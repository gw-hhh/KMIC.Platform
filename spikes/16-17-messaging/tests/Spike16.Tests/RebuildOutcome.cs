namespace Spike16.Tests;

internal sealed record RebuildOutcome(Guid RunId, bool Paused, int Confirmed, int Anomalies);
