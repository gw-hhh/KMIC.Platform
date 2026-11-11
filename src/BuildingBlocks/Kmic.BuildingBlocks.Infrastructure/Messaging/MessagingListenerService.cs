using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 随宿主启停监听线程
/// </summary>
/// <remarks>
/// 投递、处理和作业执行共用这一个监听连接
/// </remarks>
internal sealed class MessagingListenerService(MessagingListener listener) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        listener.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        listener.Stop();
        return Task.CompletedTask;
    }
}
