namespace Kmic.BuildingBlocks.Presentation.Authentication;

public static class RoleUnavailableExtensions
{
    /// <summary>允许用户在当前角色不可用时访问该端点</summary>
    public static TBuilder AllowWhenRoleUnavailable<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.WithMetadata(new AllowWhenRoleUnavailableMetadata());
    }
}
