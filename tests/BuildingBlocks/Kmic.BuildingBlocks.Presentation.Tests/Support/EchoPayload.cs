namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 覆盖各种类型的请求体，原样回显
/// </summary>
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
