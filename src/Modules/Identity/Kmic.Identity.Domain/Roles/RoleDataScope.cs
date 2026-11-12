using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Identity.Domain.Roles;

/// <summary>
/// 角色对某个范围对象单独设置的数据范围等级
/// </summary>
public sealed class RoleDataScope : Entity<RoleDataScopeId>
{
    private RoleDataScope(RoleDataScopeId id, string scopeObject, RoleScopeLevel level)
        : base(id)
    {
        ScopeObject = scopeObject;
        Level = level;
    }

    /// <summary>
    /// 范围对象编码，如 Sample.Specimen
    /// </summary>
    public string ScopeObject { get; private set; }

    /// <summary>
    /// 等级
    /// </summary>
    public RoleScopeLevel Level { get; private set; }

    internal static RoleDataScope Create(string scopeObject, RoleScopeLevel level) => new(RoleDataScopeId.New(), scopeObject, level);

    internal void ChangeLevel(RoleScopeLevel level) => Level = level;
}
