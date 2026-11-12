namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 角色授权快照：按版本缓存，内容不可变
/// </summary>
/// <param name="Version">角色的并发版本号，即授权版本号</param>
/// <param name="Permissions">拥有的权限点；全权限角色是当前目录里的全部</param>
/// <param name="DefaultScopeLevel">默认数据范围等级，数值与框架的 ScopeLevel 一致</param>
/// <param name="ScopeOverrides">按范围对象单独设置的等级，键是范围对象编码</param>
internal sealed record RoleSnapshot(
    long Version,
    IReadOnlySet<string> Permissions,
    int DefaultScopeLevel,
    IReadOnlyDictionary<string, int> ScopeOverrides);
