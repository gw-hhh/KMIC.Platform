using System.Collections.Frozen;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 角色授权快照：按版本缓存，内容不可变
/// </summary>
/// <remarks>
/// 标了不可变，HybridCache 在本地缓存里直接共享同一个实例，不做序列化；启用二级缓存（Redis）时才序列化，
/// 所以写成 JSON 能读回的形状：权限点存成数组，请求里用的集合从数组一次性建好
/// </remarks>
/// <param name="Version">角色的并发版本号，即授权版本号</param>
/// <param name="PermissionCodes">拥有的权限点编码；全权限角色是当前目录里的全部</param>
/// <param name="DefaultScopeLevel">默认数据范围等级，数值与框架的 ScopeLevel 一致</param>
/// <param name="ScopeOverrides">按范围对象单独设置的等级，键是范围对象编码</param>
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
