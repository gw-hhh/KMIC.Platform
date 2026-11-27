using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Users;

public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>按登录名查找用户，不区分大小写。</summary>
    Task<User?> FindByAccountAsync(string account, CancellationToken ct);

    /// <summary>按规范化手机号查找用户。</summary>
    Task<User?> FindByMobileAsync(string mobile, CancellationToken ct);

    /// <summary>是否存在普通用户，供首次初始化检查。</summary>
    Task<bool> AnyNormalUserAsync(CancellationToken ct);

    /// <summary>获取受保护角色持有人检查的事务锁，须在事务中调用。</summary>
    Task LockProtectedHolderChecksAsync(CancellationToken ct);
}
