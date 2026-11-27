namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

internal sealed record IdempotencySnapshot(
    Guid Id,
    byte[] RequestHash,
    string Status,
    Guid LeaseToken,
    DateTimeOffset LeaseExpiresAt,
    string? ResponseBody,
    DateTimeOffset ExpiresAt);
