namespace Kmic.BuildingBlocks.Application.Execution;

public sealed class RequestMetadata
{
    public string? IdempotencyKey { get; set; }

    public bool Replayed { get; set; }
}
