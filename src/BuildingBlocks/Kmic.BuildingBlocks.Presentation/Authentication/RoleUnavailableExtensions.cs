namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 当前角色不可用时的端点声明
/// </summary>
public static class RoleUnavailableExtensions
{
    /// <summary>
    /// 当前角色被收回后仍允许访问：只有查看自己的信息和切换角色这两个端点用
    /// </summary>
    /// <remarks>
    /// 角色被收回后，别的接口一律返回 403 Identity.CurrentRoleUnavailable，前端据此引导用户重新选择角色。
    /// 不用 401：401 会让前端去刷新令牌，而刷新解决不了角色被收回的问题
    /// </remarks>
    public static TBuilder AllowWhenRoleUnavailable<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(new AllowWhenRoleUnavailableMetadata());
    }
}
