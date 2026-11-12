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

    /// <summary>
    /// 开始“至少保留一个管理员”的检查之前先拿到这把锁，直到事务结束
    /// </summary>
    /// <remarks>
    /// 两个人同时禁用两个最后的管理员，各自都会看到对方还启用着，结果一个都不剩。所有这类检查先在这里排队，
    /// 后到的在前一个提交之后才开始检查。只在事务里调用
    /// </remarks>
    Task LockProtectedHolderChecksAsync(CancellationToken ct);
}
