using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>访问上下文加载器，由 Identity 模块实现。</summary>
public interface IAccessContextLoader
{
    /// <summary>为已认证请求加载访问上下文。</summary>
    Task<AccessLoadResult> LoadForRequestAsync(UserId user, SessionId session, RoleId role, CancellationToken ct);

    /// <summary>后台作业启动时按提交人及提交时角色重新加载上下文；用户已禁用或角色已收回时返回 null。</summary>
    Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct);
}
