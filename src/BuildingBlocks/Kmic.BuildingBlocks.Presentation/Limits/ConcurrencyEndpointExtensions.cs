namespace Kmic.BuildingBlocks.Presentation.Limits;

public static class ConcurrencyEndpointExtensions
{
    private static readonly string[] _known = [ConcurrencyPolicies.Heavy, ConcurrencyPolicies.HeavyPerUser];

    /// <summary>为统计、大范围查询等重型端点声明并发策略，超限时返回 429</summary>
    public static TBuilder RequireConcurrency<TBuilder>(this TBuilder builder, params string[] policies)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(policies);

        if (policies.Length == 0)
        {
            throw new ArgumentException("至少需指定一个并发限制策略。", nameof(policies));
        }

        var unknown = policies.FirstOrDefault(p => !_known.Contains(p, StringComparer.Ordinal));
        return unknown is null
            ? builder.WithMetadata(new ConcurrencyMetadata(policies))
            : throw new ArgumentException($"未找到名为“{unknown}”的并发限制策略，可用策略：{string.Join("、", _known)}。", nameof(policies));
    }
}
