using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles;

/// <summary>
/// 管理角色时交给资源策略的对象：角色本身，加上聚合内判断不了的事实
/// </summary>
/// <remarks>
/// 策略不访问数据库，所以“操作者当前的角色是不是受保护角色”由处理器查好后一起传入
/// </remarks>
/// <param name="Role">被管理的角色</param>
/// <param name="OperatorIsProtected">操作者当前的角色是不是受保护角色</param>
public sealed record RoleTarget(Role Role, bool OperatorIsProtected);
