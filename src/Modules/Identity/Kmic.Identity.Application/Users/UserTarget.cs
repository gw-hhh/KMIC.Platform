using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 用户管理策略的目标，包含用户及跨聚合授权事实
/// </summary>
/// <remarks>
/// 策略不访问数据库，所以这些事实由处理器查好后一起传入
/// </remarks>
/// <param name="User">被管理的用户</param>
/// <param name="OperatorIsProtected">操作者当前角色是否受保护</param>
/// <param name="HoldsProtectedRole">被管理的用户是否持有受保护角色</param>
/// <param name="IsLastProtectedHolder">目标用户是否为最后一位可登录且持有已启用受保护角色的用户</param>
public sealed record UserTarget(User User, bool OperatorIsProtected, bool HoldsProtectedRole, bool IsLastProtectedHolder);
