using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>授权管理命令执行前重新校验操作者（须在命令事务内调用）</summary>
public interface IOperatorRecheck
{
    /// <summary>加锁并校验会话、用户、角色授予及授权版本是否一致；不一致时返回 <see cref="Kmic.Identity.Domain.Errors.IdentityErrors.AuthorizationChanged" /></summary>
    Task<Result> CheckAsync(IAccessContext access, CancellationToken ct);
}
