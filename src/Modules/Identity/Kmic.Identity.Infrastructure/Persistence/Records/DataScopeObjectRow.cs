namespace Kmic.Identity.Infrastructure.Persistence.Records;

internal sealed class DataScopeObjectRow
{
    public Guid Id { get; init; }

    public required string Code { get; init; }

    public required string Module { get; set; }

    public required string DisplayName { get; set; }

    public bool IsRetired { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }
}
