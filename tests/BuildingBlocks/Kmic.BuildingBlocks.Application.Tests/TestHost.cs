using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Testing.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;

namespace Kmic.BuildingBlocks.Application.Tests;

/// <summary>
/// 相当于宿主里组装应用的部分，管道顺序和宿主一致；异常转换、幂等、事务在 Infrastructure 的测试里覆盖
/// </summary>
internal sealed class TestHost : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public TestHost()
    {
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddFakeLogging().SetMinimumLevel(LogLevel.Debug));
        services.AddSingleton<TimeProvider>(Time);
        services.AddSingleton<ISecurityEventWriter>(SecurityEvents);
        services.AddSingleton(Probe);

        var catalog = UseCaseCatalog.Build(
        [
            typeof(SubmitThingCommand),
            typeof(GetThingQuery),
            typeof(SyncThingCommand),
            typeof(LookupThingQuery),
            typeof(ChangePasswordCommand),
            typeof(SendInsideCommand),
            typeof(SlowThingQuery),
        ]);
        services.AddKmicApplication(catalog, [typeof(TestHost).Assembly]);

        // 选项必须是编译期常量，源码生成器要读取；数组顺序就是执行顺序
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(AuthorizationBehavior<,>),
            ];
        });

        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.Zero));

    public RecordingSecurityEventWriter SecurityEvents { get; } = new();

    public HandlerProbe Probe { get; } = new();

    public FakeLogCollector Logs => _provider.GetFakeLogCollector();

    public AsyncServiceScope Scope(CallOrigin origin, ICurrentUser user, IAccessContext? access = null) =>
        _provider.GetRequiredService<IExecutionScopeFactory>().CreateScope(origin, user, access);

    public AsyncServiceScope UserScope(IAccessContext access) => Scope(CallOrigin.Http, CurrentUser.From(access), access);

    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}
