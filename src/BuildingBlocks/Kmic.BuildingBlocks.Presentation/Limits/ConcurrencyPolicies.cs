namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 并发限制策略的名字
/// </summary>
public static class ConcurrencyPolicies
{
    /// <summary>
    /// 每个实例同时最多处理 <see cref="KmicHttpOptions.HeavyConcurrency"/> 个
    /// </summary>
    public const string Heavy = "Heavy";

    /// <summary>
    /// 每个用户同时只处理 1 个，没有登录的按 IP 算
    /// </summary>
    public const string HeavyPerUser = "HeavyPerUser";
}
