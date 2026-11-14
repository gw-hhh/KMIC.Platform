using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权管理命令执行前重新校验操作者
/// </summary>
/// <remarks>
/// 授权管理会产生持续生效的新授权，须防止操作者被撤权后仍扩大权限。
/// 在命令事务内按固定顺序对会话、用户、授予和角色加共享锁，再检查授权；撤权写入与共享锁互斥，使并发结果符合串行执行顺序
/// </remarks>
public interface IOperatorRecheck
{
    /// <summary>
    /// 加锁并检查：会话有效且未过期、用户启用、授予还在、角色启用且授权版本与访问上下文一致
    /// </summary>
    /// <returns>通过时成功，否则 <see cref="Kmic.Identity.Domain.Errors.IdentityErrors.AuthorizationChanged" /></returns>
    Task<Result> CheckAsync(IAccessContext access, CancellationToken ct);
}
