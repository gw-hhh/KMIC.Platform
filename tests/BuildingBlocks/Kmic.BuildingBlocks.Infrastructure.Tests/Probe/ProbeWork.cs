using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ProbeWork(AsyncServiceScope scope) : IAsyncDisposable
{
    public ProbeDbContext Db => scope.ServiceProvider.GetRequiredService<ProbeDbContext>();

    public IProbeQueryDb Query => scope.ServiceProvider.GetRequiredService<IProbeQueryDb>();

    public TicketRepository Tickets => scope.ServiceProvider.GetRequiredService<TicketRepository>();

    public ISender Sender => scope.ServiceProvider.GetRequiredService<ISender>();

    public RequestMetadata Request => scope.ServiceProvider.GetRequiredService<RequestMetadata>();

    public IServiceProvider Services => scope.ServiceProvider;

    public async Task SaveAsync()
    {
        using (scope.ServiceProvider.GetRequiredService<SaveChangesGate>().Open())
        {
            await Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    public ValueTask DisposeAsync() => scope.DisposeAsync();
}
