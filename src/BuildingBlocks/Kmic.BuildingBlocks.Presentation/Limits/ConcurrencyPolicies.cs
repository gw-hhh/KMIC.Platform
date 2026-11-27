namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>内置并发限制策略名称</summary>
public static class ConcurrencyPolicies
{
    /// <summary>单实例同时处理的重型请求总上限（见 KmicHttpOptions.HeavyConcurrency）</summary>
    public const string Heavy = "Heavy";

    /// <summary>单用户同时仅允许 1 个重型请求（匿名请求按客户端 IP 隔离）</summary>
    public const string HeavyPerUser = "HeavyPerUser";
}
