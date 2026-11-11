using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 监听数据库通知，唤醒投递和处理循环
/// </summary>
/// <remarks>
/// 每个进程一个监听连接，跑在专用线程上，用同步的 Wait(超时)：Windows 上 Npgsql 的 WaitAsync 不响应超时，
/// 兜底轮询会失效，本机行为就和生产不一样了。升级 Npgsql 时复查，修好后可以改回异步写法
/// </remarks>
internal sealed class MessagingListener(
    IOptions<DatabaseOptions> database,
    IOptions<MessagingOptions> options,
    MessagingSignals signals,
    ILogger<MessagingListener> logger) : IDisposable
{
    public const string ApplicationName = "kmic-listener";

    private static readonly TimeSpan _maxReconnectDelay = TimeSpan.FromSeconds(60);

    private readonly ManualResetEventSlim _stopping = new();
    private Thread? _thread;
    private NpgsqlConnection? _connection;

    public bool IsRunning => _thread?.IsAlive == true;

    public void Start()
    {
        if (_thread is not null)
        {
            throw new InvalidOperationException("监听已经启动。");
        }

        _thread = new Thread(Run) { IsBackground = true, Name = ApplicationName };
        _thread.Start();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "停机时关掉连接，正在等待的那一侧会抛出各种异常，这里只为结束等待")]
    public void Stop()
    {
        _stopping.Set();

        // 关掉连接才能结束正在进行的 Wait，否则要等到这一轮超时
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

    [SuppressMessage("Usage", "CA1849", Justification = "专用线程上的同步等待，原因见类型说明")]
    [SuppressMessage("Usage", "MA0042", Justification = "同上")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "连接断开、数据库重启时都要重连，监听线程不能退出")]
    private void Run()
    {
        var failures = 0;
        while (!_stopping.IsSet)
        {
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString());
                connection.Open();
                connection.Notification += (_, e) => signals.Wake(e.Channel, e.Payload);
                using (var listen = new NpgsqlCommand($"LISTEN {MessagingChannels.Outbox}; LISTEN {MessagingChannels.Inbox};", connection))
                {
                    listen.ExecuteNonQuery();
                }

                Volatile.Write(ref _connection, connection);
                failures = 0;

                // 刚连上（包括重连）时补查一次：断开期间的通知收不到
                signals.WakeAll();
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

    private string ConnectionString() =>
        new NpgsqlConnectionStringBuilder(database.Value.ConnectionString) { ApplicationName = ApplicationName, Pooling = false }.ConnectionString;
}
