namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 用例的授权声明种类，每个消息必须且只能有一种
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
