using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户的仓储
/// </summary>
public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>
    /// 按登录名查找，不区分大小写；用于登录，不存在返回空
    /// </summary>
    Task<User?> FindByAccountAsync(string account, CancellationToken ct);

    /// <summary>
    /// 按规范化后的手机号查找；用于短信登录，不存在返回空
    /// </summary>
    Task<User?> FindByMobileAsync(string mobile, CancellationToken ct);

    /// <summary>
    /// 系统里是否已经有普通用户；初始化只在没有任何用户时才能执行
    /// </summary>
    Task<bool> AnyNormalUserAsync(CancellationToken ct);
}
