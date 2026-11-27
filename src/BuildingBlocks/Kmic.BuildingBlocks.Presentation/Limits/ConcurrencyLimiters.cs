using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Presentation.Limits;

internal sealed class ConcurrencyLimiters(IOptions<KmicHttpOptions> options) : IDisposable
{
    private readonly ConcurrencyLimiter _heavy = new(new ConcurrencyLimiterOptions { PermitLimit = options.Value.HeavyConcurrency, QueueLimit = 0 });

    // 空闲分区由限流器内部自动回收
    private readonly PartitionedRateLimiter<string> _perUser = PartitionedRateLimiter.Create<string, string>(
        key => RateLimitPartition.GetConcurrencyLimiter(key, _ => new ConcurrencyLimiterOptions { PermitLimit = 1, QueueLimit = 0 }));

    public RateLimitLease TryAcquire(string policy, HttpContext http) => policy switch
    {
        ConcurrencyPolicies.Heavy => _heavy.AttemptAcquire(),
        ConcurrencyPolicies.HeavyPerUser => _perUser.AttemptAcquire(ClientKey.Of(http)),
        _ => throw new InvalidOperationException($"未找到名为“{policy}”的并发限制策略。"),
    };

    public void Dispose()
    {
        _heavy.Dispose();
        _perUser.Dispose();
    }
}
