namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed record LockedThings(IReadOnlyList<Guid> Ids);
