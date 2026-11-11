using System.Diagnostics.CodeAnalysis;
using System.Text;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// 按标签让缓存失效
/// </summary>
/// <remarks>
/// 提交之后先清本实例，再用数据库通知告诉其他实例清它们的本地缓存，不依赖 Redis。通知只在连接着的实例上有效，
/// 监听连接断开重连后，本实例清空全部本地缓存：宁可多查一次库，也不读到旧值
/// </remarks>
internal sealed class CacheInvalidator(HybridCache cache, DataSources dataSources, IOptions<CacheOptions> options, ILogger<CacheInvalidator> logger)
{
    // 标签之间的分隔符，标签本身不会含换行
    private const char Separator = '\n';

    // HybridCache 里表示“全部标签”的通配符
    private const string AllTags = "*";

    /// <summary>
    /// 本实例失效，再通知其他实例
    /// </summary>
    public async ValueTask InvalidateAsync(IReadOnlyCollection<string> tags, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(tags);
        foreach (var tag in tags)
        {
            await cache.RemoveByTagAsync(tag, ct);
        }

        await BroadcastAsync(tags, ct);
    }

    /// <summary>
    /// 收到别的实例发来的失效通知，在后台线程上处理，不占用监听线程
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "监听线程的回调不能抛异常，失效失败时靠缓存过期兜底，只记警告")]
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

    /// <summary>
    /// 监听连接重连后，断开期间的通知收不到，清空本实例全部缓存
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "同上")]
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
        }
    }

    // 标签多时分成几条通知，每条不超过上限
    private static IEnumerable<string> Split(IReadOnlyCollection<string> tags, int maxBytes)
    {
        var current = new StringBuilder();
        foreach (var tag in tags)
        {
            if (tag.Contains(Separator, StringComparison.Ordinal))
            {
                throw new ArgumentException($"缓存标签里不能有换行：“{tag}”。", nameof(tags));
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
