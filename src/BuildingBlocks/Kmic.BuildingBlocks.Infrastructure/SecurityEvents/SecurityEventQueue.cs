using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 等待写库的安全事件
/// </summary>
/// <remarks>
/// 写入只入队，不等数据库：攻击来的时候最不该让每个拒绝都同步写库。队列满了丢掉新来的并计数
/// </remarks>
internal sealed class SecurityEventQueue
{
    private readonly Channel<SecurityEventRecord> _channel;
    private long _dropped;

    public SecurityEventQueue(IOptions<SecurityEventOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // 用 Wait 模式：只调 TryWrite，满了就返回 false，不会真的等待；DropWrite 模式下满了也返回 true，没法知道有没有丢
        _channel = Channel.CreateBounded<SecurityEventRecord>(new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    /// <summary>
    /// 因为队列满了丢弃的事件数
    /// </summary>
    public long Dropped => Interlocked.Read(ref _dropped);

    public ChannelReader<SecurityEventRecord> Reader => _channel.Reader;

    /// <summary>
    /// 入队，满了返回 false
    /// </summary>
    public bool TryWrite(SecurityEventRecord record)
    {
        if (_channel.Writer.TryWrite(record))
        {
            return true;
        }

        Interlocked.Increment(ref _dropped);
        return false;
    }
}
