using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Get;

/// <summary>角色对指定数据范围对象的等级配置。</summary>
/// <param name="ScopeObject">范围对象编码</param>
/// <param name="DisplayName">显示名，对象已废弃时为空</param>
/// <param name="Level">等级</param>
public sealed record RoleDataScopeItem(string ScopeObject, string? DisplayName, RoleScopeLevel Level);
