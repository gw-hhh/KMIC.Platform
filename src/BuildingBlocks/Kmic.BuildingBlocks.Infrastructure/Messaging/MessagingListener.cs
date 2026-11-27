using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Caching;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 监听 PG NOTIFY 并唤醒对应处理循环
// 采用同步 Wait：Windows 平台下 Npgsql WaitAsync 暂不响应超时，升级驱动版本时须复查
internal sealed class MessagingListener(
    IOptions<DatabaseOptions> database,
    IOptions<MessagingOptions> options,
    MessagingSignals signals,
    CacheInvalidator cache,
    FrameworkMetrics metrics,
    ILogger<MessagingListener> logger) : IDisposable
{
    public const string ApplicationName = "kmic-listener";

    private static readonly TimeSpan _maxReconnectDelay = TimeSpan.FromSeconds(60);

    private readonly ManualResetEventSlim _stopping = new();
    private Thread? _thread;
    private NpgsqlConnection? _connection;

    public bool IsRunning => _thread?.IsAlive == true;

    public bool IsConnected => Volatile.Read(ref _connection) is not null;

    public void Start()
    {
        if (_thread is not null)
        {
            throw new InvalidOperationException("监听器已启动，请勿重复启动。");
        }

        _thread = new Thread(Run) { IsBackground = true, Name = ApplicationName };
        _thread.Start();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "停机关闭连接时阻塞等待侧会抛出异常，此处捕获仅用于正常退出等待循环")]
    public void Stop()
    {
        _stopping.Set();

        // 主动关闭连接以立即中断阻塞中的 Wait，避免等待本轮超时
        try
        {
            Volatile.Read(ref _connection)?.Dispose();
        }
        catch (Exception)
        {
        }

        _thread?.Join(TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        Stop();
        _stopping.Dispose();
    }

    [SuppressMessage("Usage", "CA1849", Justification = "专用后台线程上的同步等待，原因见类型注释")]
    [SuppressMessage("Usage", "MA0042", Justification = "专用后台线程上的同步等待，原因见类型注释")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "连接断开或数据库重启时须自动重连，监听线程不应因异常退出")]
    private void Run()
    {
        var failures = 0;
        var connectedBefore = false;
        while (!_stopping.IsSet)
        {
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString());
                connection.Open();
                connection.Notification += (_, e) => Dispatch(e.Channel, e.Payload);
                using (var listen = new NpgsqlCommand($"LISTEN {MessagingChannels.Outbox}; LISTEN {MessagingChannels.Inbox}; LISTEN {MessagingChannels.Jobs}; LISTEN {MessagingChannels.Cache};", connection))
                {
                    listen.ExecuteNonQuery();
                }

                Volatile.Write(ref _connection, connection);
                failures = 0;

                // 建连或重连成功后触发一次补偿检查，防止断连期间遗漏通知
                signals.WakeAll();
                cache.OnReconnected();
                if (connectedBefore)
                {
                    metrics.ListenerReconnected();
                }

                connectedBefore = true;
                while (!_stopping.IsSet)
                {
                    if (!connection.Wait(options.Value.PollInterval))
                    {
                        signals.WakeAll();
                    }
                }
            }
            catch (Exception exception) when (!_stopping.IsSet)
            {
                failures++;
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, failures - 1), _maxReconnectDelay.TotalSeconds));
                MessagingLog.ListenerDisconnected(logger, exception, delay.TotalSeconds);
                _stopping.Wait(delay);
            }
            catch (Exception) when (_stopping.IsSet)
            {
                return;
            }
            finally
            {
                Volatile.Write(ref _connection, null);
            }
        }
    }

    // 将缓存失效通知转发至缓存失效器，其余通知按频道唤醒对应处理循环
    private void Dispatch(string channel, string payload)
    {
        if (string.Equals(channel, MessagingChannels.Cache, StringComparison.Ordinal))
        {
            cache.OnNotification(payload);
        }
        else
        {
            signals.Wake(channel, payload);
        }
    }

    private string ConnectionString() =>
        new NpgsqlConnectionStringBuilder(database.Value.ConnectionString) { ApplicationName = ApplicationName, Pooling = false }.ConnectionString;
}
