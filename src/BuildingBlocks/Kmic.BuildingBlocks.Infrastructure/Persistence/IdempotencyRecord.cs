namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal sealed class IdempotencyRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public string Operation { get; set; } = "";

    public string Key { get; set; } = "";

    public byte[] RequestHash { get; set; } = [];

    public string Status { get; set; } = "";

    public Guid LeaseToken { get; set; }

    public DateTimeOffset LeaseExpiresAt { get; set; }

    public string? ResponseBody { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }
}
