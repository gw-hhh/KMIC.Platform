using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users;

/// <summary>用户管理策略的评估目标。</summary>
/// <param name="User">被管理的用户</param>
/// <param name="OperatorIsProtected">操作者当前角色是否受保护</param>
/// <param name="HoldsProtectedRole">持有受保护角色，即目标用户是否持有受保护角色</param>
/// <param name="IsLastProtectedHolder">最后受保护持有人，即最后一位可登录的受保护角色持有人</param>
public sealed record UserTarget(User User, bool OperatorIsProtected, bool HoldsProtectedRole, bool IsLastProtectedHolder);
