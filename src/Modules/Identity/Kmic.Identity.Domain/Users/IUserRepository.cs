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
    /// 按规范化手机号查找，不存在时返回 null
    /// </summary>
    Task<User?> FindByMobileAsync(string mobile, CancellationToken ct);

    /// <summary>
    /// 是否已有普通用户，供首次初始化检查
    /// </summary>
    Task<bool> AnyNormalUserAsync(CancellationToken ct);

    /// <summary>
    /// 获取受保护角色持有人检查的事务锁
    /// </summary>
    /// <remarks>
    /// 必须在事务中调用，锁持有至事务结束，使并发检查依次执行，防止同时操作后失去最后一位可登录的持有人
    /// </remarks>
    Task LockProtectedHolderChecksAsync(CancellationToken ct);
}
