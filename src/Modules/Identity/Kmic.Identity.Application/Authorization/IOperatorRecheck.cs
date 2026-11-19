using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权管理命令执行前重新校验操作者，防止已被撤权的人继续授权
/// </summary>
/// <remarks>
/// 须在命令事务里加共享锁再检查，与撤权写入互斥
/// </remarks>
public interface IOperatorRecheck
{
    /// <summary>
    /// 加锁并检查：会话有效且未过期、用户启用、授予还在、角色启用且授权版本与访问上下文一致
    /// </summary>
    /// <returns>通过时成功，否则 <see cref="Kmic.Identity.Domain.Errors.IdentityErrors.AuthorizationChanged" /></returns>
    Task<Result> CheckAsync(IAccessContext access, CancellationToken ct);
}
