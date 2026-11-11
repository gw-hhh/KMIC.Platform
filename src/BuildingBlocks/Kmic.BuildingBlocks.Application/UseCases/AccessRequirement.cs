namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 授权声明类型
/// </summary>
public enum AccessRequirement
{
    /// <summary>
    /// [RequirePermission]
    /// </summary>
    Permission,

    /// <summary>
    /// [AllowAuthenticated]
    /// </summary>
    Authenticated,

    /// <summary>
    /// [AllowAnonymous]
    /// </summary>
    Anonymous,

    /// <summary>
    /// [SystemOnly]
    /// </summary>
    SystemOnly,

    /// <summary>
    /// [ContractQuery]
    /// </summary>
    ContractQuery,
}
