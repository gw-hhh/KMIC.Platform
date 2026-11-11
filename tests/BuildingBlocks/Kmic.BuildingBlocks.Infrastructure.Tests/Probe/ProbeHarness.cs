using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 按真实宿主的方式注册 Probe 模块的数据访问；每个工作单元一个 DI 作用域
/// </summary>
internal sealed class ProbeHarness : IAsyncDisposable
{
    private readonly ServiceProvider _services;

    public ProbeHarness(string connectionString, Action<DatabaseOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Time);
        services.AddKmicApplication([typeof(ProbeScopes).Assembly]);
        services.AddKmicPersistence(o =>
        {
            o.ConnectionString = connectionString;
            configure?.Invoke(o);
        });
        services.AddModuleDbContexts<ProbeDbContext, ProbeQueryDbContext, IProbeQueryDb>();
        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 10, 2, 0, 0, TimeSpan.Zero));

    /// <summary>
    /// 宿主的服务，测试中直接取框架内部服务（如数据源）时用
    /// </summary>
    public IServiceProvider Services => _services;

    private IExecutionScopeFactory Factory => _services.GetRequiredService<IExecutionScopeFactory>();

    public ProbeWork Begin(TestIdentity identity) => new(Factory.CreateScope(identity));

    public ProbeWork BeginAsSystem() => new(Factory.CreateSystemScope());

    public ProbeWork Begin(CallOrigin origin, ICurrentUser user) => new(Factory.CreateScope(origin, user));

    public async Task<Ticket> AddAsync(TestIdentity identity, Ticket ticket)
    {
        await using var work = Begin(identity);
        work.Db.Add(ticket);
        await work.SaveAsync();
        return ticket;
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();
}
