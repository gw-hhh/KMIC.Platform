using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 加载访问上下文，由 Identity 实现
/// </summary>
public interface IAccessContextLoader
{
    /// <summary>
    /// 为已认证的请求加载访问上下文
    /// </summary>
    Task<AccessLoadResult> LoadForRequestAsync(UserId user, SessionId session, RoleId role, CancellationToken ct);

    /// <summary>
    /// 后台作业开始执行时，按提交人和提交时的角色重新加载
    /// </summary>
    /// <remarks>
    /// 用户已禁用或角色已收回时返回空
    /// </remarks>
    Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct);
}
