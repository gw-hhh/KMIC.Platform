namespace Kmic.Identity.Infrastructure.Persistence.Records;

internal sealed class CatalogStateRow
{
    public required string Name { get; init; }

    public required string ContentHash { get; set; }

    public DateTimeOffset SyncedAt { get; set; }
}
