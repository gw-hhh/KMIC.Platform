using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Get;

/// <summary>
/// 角色详情里一个范围对象的单独设置
/// </summary>
/// <param name="ScopeObject">范围对象编码</param>
/// <param name="DisplayName">显示名；对象已退役时为空</param>
/// <param name="Level">等级</param>
public sealed record RoleDataScopeItem(string ScopeObject, string? DisplayName, RoleScopeLevel Level);
