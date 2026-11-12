using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>
/// 管理用户时交给资源策略的对象：用户本身，加上聚合内判断不了的事实
/// </summary>
/// <remarks>
/// 策略不访问数据库，所以这些事实由处理器查好后一起传入
/// </remarks>
/// <param name="User">被管理的用户</param>
/// <param name="OperatorIsProtected">操作者当前的角色是不是受保护角色</param>
/// <param name="HoldsProtectedRole">被管理的用户是否持有受保护角色</param>
/// <param name="IsLastProtectedHolder">被管理的用户是否是最后一个启用的受保护角色持有人</param>
public sealed record UserTarget(User User, bool OperatorIsProtected, bool HoldsProtectedRole, bool IsLastProtectedHolder);
