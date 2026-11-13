using System.Threading.RateLimiting;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Operations.Application.ClientErrors;
using Microsoft.Extensions.Options;

namespace Kmic.Operations.Infrastructure.ClientErrors;

/// <summary>
/// 按用户的固定窗口限流，窗口一分钟；每个实例各自计数
/// </summary>
/// <remarks>
/// 用户很多时分区不会无限增长：空闲的分区由限流器自己回收。前端错误上报是防刷，不是计费，
/// 多实例各数各的可以接受，所以不放进数据库或缓存
/// </remarks>
internal sealed class ClientErrorLimiter : IClientErrorLimiter, IDisposable
{
    private readonly PartitionedRateLimiter<Guid> _limiter;

    public ClientErrorLimiter(IOptions<ClientErrorOptions> options)
    {
        var limit = options.Value.RequestsPerUserPerMinute;
        _limiter = PartitionedRateLimiter.Create<Guid, Guid>(user => RateLimitPartition.GetFixedWindowLimiter(user, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
    }

    public TimeSpan? TryAcquire(UserId user)
    {
        using var lease = _limiter.AttemptAcquire(user.Value);
        if (lease.IsAcquired)
        {
            return null;
        }

        return lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) ? retryAfter : TimeSpan.FromMinutes(1);
    }

    public void Dispose() => _limiter.Dispose();
}
