namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 当前角色不可用时的端点声明
/// </summary>
public static class RoleUnavailableExtensions
{
    /// <summary>
    /// 当前角色被收回后仍允许访问，只给查看自己的信息和切换角色用
    /// </summary>
    public static TBuilder AllowWhenRoleUnavailable<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(new AllowWhenRoleUnavailableMetadata());
    }
}
