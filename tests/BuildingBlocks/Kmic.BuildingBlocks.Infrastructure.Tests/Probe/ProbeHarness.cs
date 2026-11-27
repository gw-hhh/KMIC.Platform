using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Infrastructure.External;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Security;
using Kmic.Echo.Probe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

// 按真实宿主方式注册 Probe、Echo 模块，每个工作单元对应独立 DI 作用域
internal sealed class ProbeHarness : IAsyncDisposable
{
    public const string MirrorSubscription = "Echo.MirrorTicket";

    public const string DailyStamp = "BuildingBlocks.DailyStamp";

    private readonly ServiceProvider _services;

    public ProbeHarness(
        string connectionString,
        Action<DatabaseOptions>? configure = null,
        Action<MessagingOptions>? messaging = null,
        Action<IServiceCollection>? services = null,
        bool withRebuild = false)
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<TimeProvider>(Time);
        collection.AddSingleton<ISecurityEventWriter>(SecurityEvents);
        collection.AddKmicApplication([typeof(ProbeScopes).Assembly, typeof(EchoHooks).Assembly]);

        AddEchoSubscription(collection, withRebuild);
        collection.AddSingleton(Echo);
        collection.AddKmicHttpClient<IPartnerClient, PartnerClient>("Partner")
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://partner.example.com"))
            .ConfigurePrimaryHttpMessageHandler(() => PartnerServer);
        collection.AddKmicScheduling("BuildingBlocks", s => s.Schedule(
            DailyStamp, "0 0 7 * * ?", run => new StampTicketsCommand(run.ScheduledFireTime), o => o.DisplayName("每日盖章")));
        collection.AddKmicStartupTasks();
        collection.AddKmicMessagingWorker();
        collection.AddKmicJobWorker();
        collection.AddKmicScheduler();
        collection.AddSingleton<IAccessContextLoader>(Access);
        services?.Invoke(collection);
        collection.AddKmicPersistence(o =>
        {
            o.ConnectionString = connectionString;
            o.CheckQueryVisibility = true;
            configure?.Invoke(o);
        });
        collection.AddModuleDbContexts<ProbeDbContext, ProbeQueryDbContext, IProbeQueryDb>();
        collection.AddModuleDbContexts<EchoDbContext, EchoQueryDbContext, IEchoQueryDb>();
        collection.Configure<MessagingOptions>(o => messaging?.Invoke(o));
        collection.AddScoped<TicketRepository>();
        collection.AddSingleton(Hooks);
        collection.AddLogging();

        // 选项需为编译期常量以供源码生成器读取，数组顺序即管道执行顺序
        collection.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(ExceptionTranslationBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(AuthorizationBehavior<,>),
                typeof(ReadAuditBehavior<,>),
                typeof(IdempotencyBehavior<,>),
                typeof(TransactionBehavior<,>),
            ];
        });
        _services = collection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 10, 2, 0, 0, TimeSpan.Zero));

    public RecordingSecurityEventWriter SecurityEvents { get; } = new();

    public ProbeHooks Hooks { get; } = new();

    public EchoHooks Echo { get; } = new();

    public FakePartnerServer PartnerServer { get; } = new(System.Net.HttpStatusCode.OK);

    public FakeTicketSource TicketSource { get; } = new();

    public TestAccessContextLoader Access { get; } = new();

    public IServiceProvider Services => _services;

    private IExecutionScopeFactory Factory => _services.GetRequiredService<IExecutionScopeFactory>();

    public ProbeWork Begin(TestIdentity identity) => new(Factory.CreateScope(identity));

    public ProbeWork Begin(TestIdentity identity, CallOrigin origin) => new(Factory.CreateScope(identity, origin));

    public ProbeWork BeginAsSystem() => new(Factory.CreateSystemScope());

    public ProbeWork Begin(CallOrigin origin, ICurrentUser user) => new(Factory.CreateScope(origin, user));

    public async Task<Ticket> AddAsync(TestIdentity identity, Ticket ticket)
    {
        await using var work = Begin(identity);
        work.Db.Add(ticket);
        await work.SaveAsync();
        return ticket;
    }

    // 按注册顺序启动后台服务：先执行启动任务，再启动后台循环
    public async Task StartHostedServicesAsync(CancellationToken ct)
    {
        foreach (var service in _services.GetServices<IHostedService>())
        {
            await service.StartAsync(ct);
        }
    }

    public async Task StopHostedServicesAsync(CancellationToken ct)
    {
        foreach (var service in _services.GetServices<IHostedService>().Reverse())
        {
            await service.StopAsync(ct);
        }
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    // Echo 有序处理 Probe 发布的单据快照，可重建时还声明怎样从来源重建读模型
    private void AddEchoSubscription(ServiceCollection collection, bool withRebuild)
    {
        collection.AddKmicSubscriptions("Echo", m => m.Subscribe<TicketChanged, MirrorTicketCommand>(
            MirrorSubscription,
            (e, envelope) => new MirrorTicketCommand(e.Ticket.Id, e.Ticket.Title, envelope.SourceVersion ?? 0),
            o =>
            {
                o.Ordered();
                if (withRebuild)
                {
                    o.RebuildWith<ITicketSnapshotSource, TicketSnapshot, TicketSummary>(item => new TicketSummary
                    {
                        Id = item.Snapshot.Id,
                        SourceVersion = item.SourceVersion,
                        Status = ReferenceStatus.Active,
                        Title = item.Snapshot.Title,
                    });
                }
            }));
        collection.AddSingleton<ITicketSnapshotSource>(TicketSource);
    }
}
