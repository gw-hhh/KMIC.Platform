namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 幂等记录
/// </summary>
internal sealed record IdempotencySnapshot(
    Guid Id,
    byte[] RequestHash,
    string Status,
    Guid LeaseToken,
    DateTimeOffset LeaseExpiresAt,
    string? ResponseBody,
    DateTimeOffset ExpiresAt);
