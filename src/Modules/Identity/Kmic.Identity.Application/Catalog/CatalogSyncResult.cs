namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 一次目录同步的结果
/// </summary>
/// <param name="Changed">目录有变化并已同步；哈希没变为否</param>
/// <param name="Changes">数据库里的目录改了什么；没有变化时为空</param>
public sealed record CatalogSyncResult(bool Changed, CatalogChanges? Changes);
