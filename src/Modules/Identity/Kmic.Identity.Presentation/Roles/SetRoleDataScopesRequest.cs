using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 替换角色数据范围的请求
/// </summary>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回 409 Common.VersionConflict</param>
/// <param name="DefaultLevel">默认等级</param>
/// <param name="Overrides">按范围对象单独设置的等级，键是范围对象编码</param>
public sealed record SetRoleDataScopesRequest(long ExpectedVersion, RoleScopeLevel DefaultLevel, IReadOnlyDictionary<string, RoleScopeLevel> Overrides);
