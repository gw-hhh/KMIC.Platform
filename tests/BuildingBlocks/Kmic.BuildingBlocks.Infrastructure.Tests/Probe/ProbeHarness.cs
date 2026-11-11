using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 按真实宿主的方式注册 Probe 模块，每个工作单元一个依赖注入作用域
/// </summary>
internal sealed class ProbeHarness : IAsyncDisposable
{
    private readonly ServiceProvider _services;

    public ProbeHarness(string connectionString, Action<DatabaseOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton<ISecurityEventWriter>(SecurityEvents);
        services.AddKmicApplication([typeof(ProbeScopes).Assembly]);
        services.AddKmicPersistence(o =>
        {
            o.ConnectionString = connectionString;
            o.CheckQueryVisibility = true;
            configure?.Invoke(o);
        });
        services.AddModuleDbContexts<ProbeDbContext, ProbeQueryDbContext, IProbeQueryDb>();
        services.AddScoped<TicketRepository>();
        services.AddScoped<IDomainEventHandler<TicketClosed>, TicketClosedHandler>();
        services.AddSingleton(Hooks);
        services.AddLogging();

        // 选项必须是编译期常量，源码生成器要读取；数组顺序就是执行顺序
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(ExceptionTranslationBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(AuthorizationBehavior<,>),
                typeof(IdempotencyBehavior<,>),
                typeof(TransactionBehavior<,>),
            ];
        });
        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 10, 2, 0, 0, TimeSpan.Zero));

    public RecordingSecurityEventWriter SecurityEvents { get; } = new();

    public ProbeHooks Hooks { get; } = new();

    /// <summary>
    /// 宿主的服务，测试要直接取框架内部服务（如数据源）时用
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
