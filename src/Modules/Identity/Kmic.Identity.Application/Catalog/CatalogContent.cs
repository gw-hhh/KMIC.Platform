using System.Security.Cryptography;
using System.Text;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 代码里声明的权限点与范围对象：要同步进数据库的内容
/// </summary>
public sealed class CatalogContent
{
    private CatalogContent(IReadOnlyList<PermissionDefinition> permissions, IReadOnlyList<DataScopeObjectDefinition> scopeObjects)
    {
        Permissions = permissions;
        ScopeObjects = scopeObjects;
    }

    /// <summary>
    /// 权限点，按编码排序
    /// </summary>
    public IReadOnlyList<PermissionDefinition> Permissions { get; }

    /// <summary>
    /// 范围对象，按编码排序
    /// </summary>
    public IReadOnlyList<DataScopeObjectDefinition> ScopeObjects { get; }

    /// <summary>
    /// 从两个目录读出
    /// </summary>
    public static CatalogContent From(PermissionCatalog permissions, DataScopeObjectCatalog scopeObjects)
    {
        ArgumentNullException.ThrowIfNull(permissions);
        ArgumentNullException.ThrowIfNull(scopeObjects);

        return new CatalogContent(
            [.. permissions.All.OrderBy(p => p.Code, StringComparer.Ordinal)],
            [.. scopeObjects.All.OrderBy(s => s.Code, StringComparer.Ordinal)]);
    }

    /// <summary>
    /// 内容的哈希（SHA-256，小写十六进制）：编码、显示名、敏感与改名信息有任何变化，哈希就变
    /// </summary>
    public string Hash()
    {
        var text = new StringBuilder();
        foreach (var p in Permissions)
        {
            text.Append("P|").Append(p.Code).Append('|').Append(p.Module).Append('|').Append(p.Group).Append('|').Append(p.DisplayName)
                .Append('|').Append(p.Sensitive ? '1' : '0').Append('|').Append(p.RenamedFrom).Append('\n');
        }

        foreach (var s in ScopeObjects)
        {
            text.Append("S|").Append(s.Code).Append('|').Append(s.Module).Append('|').Append(s.DisplayName).Append('\n');
        }

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));
    }
}
