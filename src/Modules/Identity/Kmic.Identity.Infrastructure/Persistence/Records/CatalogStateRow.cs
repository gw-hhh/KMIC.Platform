namespace Kmic.Identity.Infrastructure.Persistence.Records;

/// <summary>
/// 目录同步状态表的一行：上次同步的目录哈希与时间，哈希没变就跳过同步
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
