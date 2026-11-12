namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 一次同步对数据库里的目录改了什么
/// </summary>
/// <param name="PermissionsAdded">新增的权限点</param>
/// <param name="PermissionsUpdated">显示名、分组或敏感标记变了的权限点（含从退役恢复的）</param>
/// <param name="PermissionsRetired">代码里已经不存在、这次标为退役的权限点</param>
/// <param name="ScopeObjectsAdded">新增的范围对象</param>
/// <param name="ScopeObjectsRetired">这次标为退役的范围对象</param>
/// <param name="Renames">代码里声明的改名：旧编码到新编码</param>
public sealed record CatalogChanges(
    int PermissionsAdded,
    int PermissionsUpdated,
    int PermissionsRetired,
    int ScopeObjectsAdded,
    int ScopeObjectsRetired,
    IReadOnlyDictionary<string, string> Renames);
