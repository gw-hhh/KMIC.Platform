using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

internal sealed class SecurityEventQueue
{
    private readonly Channel<SecurityEventRecord> _channel;
    private long _dropped;

    public SecurityEventQueue(IOptions<SecurityEventOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // 使用 Wait 模式：仅调用 TryWrite，队列满时返回 false 且不阻塞；DropWrite 模式下队满仍返回 true，无法判断是否发生丢弃
        _channel = Channel.CreateBounded<SecurityEventRecord>(new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    public long Dropped => Interlocked.Read(ref _dropped);

    public ChannelReader<SecurityEventRecord> Reader => _channel.Reader;

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
