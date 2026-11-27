using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

// 带命中统计的 HybridCache 包装：仅当执行加载回调时记为未命中
internal sealed class MeteredHybridCache(HybridCache inner, FrameworkMetrics metrics) : HybridCache
{
    public override async ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var load = new Load<TState, T>(state, factory);
        var value = await inner.GetOrCreateAsync(key, load, static (l, ct) => l.RunAsync(ct), options, tags, cancellationToken);
        metrics.CacheRequest(CacheKey.CategoryOf(key), hit: !load.Ran);
        return value;
    }

    public override ValueTask SetAsync<T>(
        string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) =>
        inner.SetAsync(key, value, options, tags, cancellationToken);

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        inner.RemoveAsync(key, cancellationToken);

    public override ValueTask RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default) =>
        inner.RemoveAsync(keys, cancellationToken);

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) =>
        inner.RemoveByTagAsync(tag, cancellationToken);

    public override ValueTask RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default) =>
        inner.RemoveByTagAsync(tags, cancellationToken);

    private sealed class Load<TState, T>(TState state, Func<TState, CancellationToken, ValueTask<T>> factory)
    {
        public bool Ran { get; private set; }

        public ValueTask<T> RunAsync(CancellationToken ct)
        {
            Ran = true;
            return factory(state, ct);
        }
    }
}
