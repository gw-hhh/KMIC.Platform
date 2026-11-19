using System.Collections.Frozen;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 角色授权快照：按版本缓存，内容不可变
/// </summary>
/// <param name="Version">角色的并发版本号</param>
/// <param name="PermissionCodes">拥有的权限点编码</param>
/// <param name="DefaultScopeLevel">默认数据范围等级</param>
/// <param name="ScopeOverrides">按范围对象单独设置的等级</param>
[ImmutableObject(true)]
internal sealed record RoleSnapshot(long Version, IReadOnlyList<string> PermissionCodes, int DefaultScopeLevel, IReadOnlyDictionary<string, int> ScopeOverrides)
{
    private readonly Lazy<FrozenSet<string>> _permissions = new(() => PermissionCodes.ToFrozenSet(StringComparer.Ordinal));

    /// <summary>
    /// 请求里用的权限点集合
    /// </summary>
    [JsonIgnore]
    public IReadOnlySet<string> Permissions => _permissions.Value;
}
