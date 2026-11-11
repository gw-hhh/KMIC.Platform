namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 端点上的并发限制声明
/// </summary>
public static class ConcurrencyEndpointExtensions
{
    private static readonly string[] _known = [ConcurrencyPolicies.Heavy, ConcurrencyPolicies.HeavyPerUser];

    /// <summary>
    /// 声明这是重型接口，并发超出限制时立即返回 429，不排队
    /// </summary>
    /// <remarks>
    /// 统计、大范围查询、同步预览这类占用大量数据库或内存的端点必须声明。
    /// 并发许可覆盖整个请求处理过程，流式响应写完才释放
    /// </remarks>
    /// <param name="builder">端点</param>
    /// <param name="policies">策略名，见 <see cref="ConcurrencyPolicies"/>，可以同时用多个</param>
    public static TBuilder RequireConcurrency<TBuilder>(this TBuilder builder, params string[] policies)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(policies);

        if (policies.Length == 0)
        {
            throw new ArgumentException("至少要给出一个并发限制策略。", nameof(policies));
        }

        var unknown = policies.FirstOrDefault(p => !_known.Contains(p, StringComparer.Ordinal));
        return unknown is null
            ? builder.WithMetadata(new ConcurrencyMetadata(policies))
            : throw new ArgumentException($"没有名为“{unknown}”的并发限制策略，可用：{string.Join("、", _known)}。", nameof(policies));
    }
}
