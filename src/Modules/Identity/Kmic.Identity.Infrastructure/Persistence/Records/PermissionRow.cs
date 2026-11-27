namespace Kmic.Identity.Infrastructure.Persistence.Records;

internal sealed class PermissionRow
{
    public Guid Id { get; init; }

    // 权限编码，改名时原地更新
    public required string Code { get; set; }

    public required string Module { get; set; }

    public required string GroupName { get; set; }

    public required string DisplayName { get; set; }

    public bool IsSensitive { get; set; }

    public bool IsRetired { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }
}
