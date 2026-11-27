using Microsoft.Extensions.Hosting;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

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
