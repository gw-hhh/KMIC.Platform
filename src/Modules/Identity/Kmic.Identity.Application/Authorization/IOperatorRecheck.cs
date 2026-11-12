using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权管理命令提交前对操作者的重查
/// </summary>
/// <remarks>
/// 撤权的生效边界：普通业务命令在“请求开始时加载的授权”之后的窗口里，影响只是一次业务操作；
/// 授权管理命令的结果是新的授权，会一直存在，还能继续放大。所以这类命令在同一个事务里，按固定顺序对操作者的
/// 会话、用户、授予、角色加共享锁再检查一遍，撤权的写入与共享锁冲突，结果一定是某个串行顺序
/// </remarks>
public interface IOperatorRecheck
{
    /// <summary>
    /// 加锁并检查：会话有效且未过期、用户启用、授予还在、角色启用且授权版本与访问上下文一致
    /// </summary>
    /// <returns>通过时成功，否则 <see cref="Kmic.Identity.Domain.Errors.IdentityErrors.AuthorizationChanged" /></returns>
    Task<Result> CheckAsync(IAccessContext access, CancellationToken ct);
}
