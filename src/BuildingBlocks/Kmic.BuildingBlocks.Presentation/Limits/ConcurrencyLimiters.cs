using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 并发限制：每个实例一个总量限制，每个请求方一个单并发限制
/// </summary>
/// <remarks>
/// 超出立即拒绝，不排队。不用 ASP.NET Core 内置的限流中间件做这件事：它一个端点只能用一个策略，
/// 而重型接口要同时满足“实例总量”和“每用户一个”
/// </remarks>
internal sealed class ConcurrencyLimiters(IOptions<KmicHttpOptions> options) : IDisposable
{
    private readonly ConcurrencyLimiter _heavy = new(new ConcurrencyLimiterOptions { PermitLimit = options.Value.HeavyConcurrency, QueueLimit = 0 });

    // 分区空闲后由限流器自己回收
    private readonly PartitionedRateLimiter<string> _perUser = PartitionedRateLimiter.Create<string, string>(
        key => RateLimitPartition.GetConcurrencyLimiter(key, _ => new ConcurrencyLimiterOptions { PermitLimit = 1, QueueLimit = 0 }));

    public RateLimitLease TryAcquire(string policy, HttpContext http) => policy switch
    {
        ConcurrencyPolicies.Heavy => _heavy.AttemptAcquire(),
        ConcurrencyPolicies.HeavyPerUser => _perUser.AttemptAcquire(ClientKey.Of(http)),
        _ => throw new InvalidOperationException($"没有名为“{policy}”的并发限制策略。"),
    };

    public void Dispose()
    {
        _heavy.Dispose();
        _perUser.Dispose();
    }
}
