using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 一个作用域里的写入上下文和查询上下文；SaveAsync 相当于命令事务的保存
/// </summary>
internal sealed class ProbeWork(AsyncServiceScope scope) : IAsyncDisposable
{
    public ProbeDbContext Db => scope.ServiceProvider.GetRequiredService<ProbeDbContext>();

    public IProbeQueryDb Query => scope.ServiceProvider.GetRequiredService<IProbeQueryDb>();

    public TicketRepository Tickets => scope.ServiceProvider.GetRequiredService<TicketRepository>();

    public ISender Sender => scope.ServiceProvider.GetRequiredService<ISender>();

    public RequestMetadata Request => scope.ServiceProvider.GetRequiredService<RequestMetadata>();

    public async Task SaveAsync()
    {
        using (scope.ServiceProvider.GetRequiredService<SaveChangesGate>().Open())
        {
            await Db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    public ValueTask DisposeAsync() => scope.DisposeAsync();
}
