namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

internal sealed record EchoPayload(
    ThingId Id,
    ThingStatus Status,
    DateTimeOffset At,
    DateOnly Day,
    TimeOnly Time,
    decimal Amount,
    string Name,
    string? Remark,
    int Count);
