namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 目录同步的变更明细
/// </summary>
/// <param name="PermissionsAdded">新增权限点数</param>
/// <param name="PermissionsUpdated">更新的权限点数</param>
/// <param name="PermissionsRetired">本次退役的权限点数</param>
/// <param name="ScopeObjectsAdded">新增数据范围对象数</param>
/// <param name="ScopeObjectsRetired">本次退役的数据范围对象数</param>
/// <param name="Renames">代码里声明的改名</param>
public sealed record CatalogChanges(
    int PermissionsAdded,
    int PermissionsUpdated,
    int PermissionsRetired,
    int ScopeObjectsAdded,
    int ScopeObjectsRetired,
    IReadOnlyDictionary<string, string> Renames);
