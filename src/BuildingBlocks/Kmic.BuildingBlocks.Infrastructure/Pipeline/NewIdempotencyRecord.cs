namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 新的幂等记录
/// </summary>
internal sealed record NewIdempotencyRecord
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required Guid RoleId { get; init; }

    public required string Operation { get; init; }

    public required string Key { get; init; }

    public required byte[] RequestHash { get; init; }

    public required Guid LeaseToken { get; init; }

    public required DateTimeOffset LeaseExpiresAt { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
