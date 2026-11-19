using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Application.Observability;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// 按缓存键的类别记录命中次数的 HybridCache，本次调用执行了加载方法才算未命中
/// </summary>
/// <remarks>
/// 并发等待同一次加载的调用也算命中
/// </remarks>
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
