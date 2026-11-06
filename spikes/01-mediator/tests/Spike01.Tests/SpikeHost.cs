using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Spike01.BuildingBlocks;
using Spike01.Customers;

namespace Spike01.Tests;

// 相当于宿主（Kmic.Api）中注册 Mediator 的地方：只有这里引用 Mediator.SourceGenerator，分发代码生成在这个程序集中
internal static class SpikeHost
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 11, 6, 0, 0, 0, TimeSpan.Zero)));
        services.AddSpikeBuildingBlocks();
        services.AddCustomersModule();

        // 选项必须是编译期常量，由源码生成器在编译时读取；管道顺序在这里一次性声明（05-02 第 4.1 节）
        services.AddMediator((MediatorOptions options) =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.GenerateTypesAsInternal = true;
            options.PipelineBehaviors =
            [
                typeof(LoggingBehavior<,>),
                typeof(ValidationBehavior<,>),
                typeof(AuthorizationBehavior<,>),
                typeof(TransactionBehavior<,>),
            ];
        });

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }
}
