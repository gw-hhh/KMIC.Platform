namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 目录同步状态
/// </summary>
internal sealed class CatalogStateRow
{
    /// <summary>
    /// 目录名，只有一行，叫 Permissions
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 上次同步的目录内容哈希
    /// </summary>
    public required string ContentHash { get; set; }

    /// <summary>
    /// 上次同步的时间
    /// </summary>
    public DateTimeOffset SyncedAt { get; set; }
}
