namespace Kmic.Identity.Application.Catalog;

/// <summary>目录同步结果</summary>
/// <param name="Changed">目录是否有变化并已同步</param>
/// <param name="Changes">目录变更明细，无变化时为空</param>
public sealed record CatalogSyncResult(bool Changed, CatalogChanges? Changes);
