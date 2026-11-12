using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 加载访问上下文
/// </summary>
/// <remarks>
/// 由 Identity 模块实现。框架不认识 Identity 的表，只依赖这个接口
/// </remarks>
public interface IAccessContextLoader
{
    /// <summary>
    /// 每个已认证的请求，按令牌里的用户、会话和当前角色加载
    /// </summary>
    /// <remarks>
    /// 状态查询每个请求一次；角色授权快照和组织树快照按版本缓存，不随请求查库
    /// </remarks>
    Task<AccessLoadResult> LoadForRequestAsync(UserId user, SessionId session, RoleId role, CancellationToken ct);

    /// <summary>
    /// 后台作业开始执行时，按提交人和提交时的角色重新加载
    /// </summary>
    /// <remarks>
    /// 用户已禁用、角色已收回或停用时返回空，作业就不执行。不需要会话
    /// </remarks>
    Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct);
}
