using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Presentation.Roles;

/// <summary>
/// 替换角色数据范围的请求
/// </summary>
/// <param name="ExpectedVersion">预期版本号</param>
/// <param name="DefaultLevel">默认等级</param>
/// <param name="Overrides">按范围对象单独设置的等级</param>
public sealed record SetRoleDataScopesRequest(long ExpectedVersion, RoleScopeLevel DefaultLevel, IReadOnlyDictionary<string, RoleScopeLevel> Overrides);
