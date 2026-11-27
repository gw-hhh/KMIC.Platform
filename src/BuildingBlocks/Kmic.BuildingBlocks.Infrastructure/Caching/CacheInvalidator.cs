using System.Diagnostics.CodeAnalysis;
using System.Text;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

// 基于标签的分布式缓存失效器（本地移除后经 PG NOTIFY 广播）
internal sealed class CacheInvalidator(
    HybridCache cache, DataSources dataSources, IOptions<CacheOptions> options, FrameworkMetrics metrics, ILogger<CacheInvalidator> logger)
{
    private const char Separator = '\n';
    private const string AllTags = "*";

    public async ValueTask InvalidateAsync(IReadOnlyCollection<string> tags, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(tags);
        foreach (var tag in tags)
        {
            await cache.RemoveByTagAsync(tag, ct);
        }

        await BroadcastAsync(tags, ct);
    }

    // 投递到后台线程池，避免阻塞 PG 监听连接
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "监听回调不允许向外抛出异常，失效失败仅记录警告并依赖缓存过期兜底")]
    public void OnNotification(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        _ = Task.Run(async () =>
        {
            try
            {
                foreach (var tag in payload.Split(Separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    await cache.RemoveByTagAsync(tag);
                }
            }
            catch (Exception exception)
            {
                CacheLog.RemoteInvalidationFailed(logger, exception);
            }
        });
    }

    // 断连期间可能漏掉通知，重连后清空全部标签兜底
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "监听重连回调不允许向外抛出异常，失效失败仅记录警告并依赖缓存过期兜底")]
    public void OnReconnected() =>
        _ = Task.Run(async () =>
        {
            try
            {
                await cache.RemoveByTagAsync(AllTags);
            }
            catch (Exception exception)
            {
                CacheLog.RemoteInvalidationFailed(logger, exception);
            }
        });

    private async ValueTask BroadcastAsync(IReadOnlyCollection<string> tags, CancellationToken ct)
    {
        if (tags.Count == 0)
        {
            return;
        }

        await using var connection = await dataSources.Transactions(CallOrigin.BackgroundJob).OpenConnectionAsync(ct);
        foreach (var payload in Split(tags, options.Value.MaxNotifyBytes))
        {
            await using var command = new NpgsqlCommand("SELECT pg_notify(@channel, @payload)", connection);
            command.Parameters.AddWithValue("channel", MessagingChannels.Cache);
            command.Parameters.AddWithValue("payload", payload);
            await command.ExecuteNonQueryAsync(ct);
            metrics.CacheInvalidationBroadcast();
        }
    }

    // 标签过多时分批发送通知，确保单条不超过长度上限
    private static IEnumerable<string> Split(IReadOnlyCollection<string> tags, int maxBytes)
    {
        var current = new StringBuilder();
        foreach (var tag in tags)
        {
            if (tag.Contains(Separator, StringComparison.Ordinal))
            {
                throw new ArgumentException($"缓存标签不能包含换行符：“{tag}”。", nameof(tags));
            }

            if (current.Length > 0 && Encoding.UTF8.GetByteCount(current.ToString()) + 1 + Encoding.UTF8.GetByteCount(tag) > maxBytes)
            {
                yield return current.ToString();
                current.Clear();
            }

            if (current.Length > 0)
            {
                current.Append(Separator);
            }

            current.Append(tag);
        }

        if (current.Length > 0)
        {
            yield return current.ToString();
        }
    }
}
