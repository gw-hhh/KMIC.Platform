using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>测试身份执行作用域扩展。</summary>
public static class TestIdentityExtensions
{
    /// <summary>以指定测试身份创建执行作用域（默认来源为 HTTP，消息经完整管道处理）。</summary>
    public static AsyncServiceScope CreateScope(this IExecutionScopeFactory factory, TestIdentity identity, CallOrigin origin = CallOrigin.Http)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(identity);
        return factory.CreateScope(origin, CurrentUser.From(identity), identity);
    }

    /// <summary>以系统身份创建执行作用域。</summary>
    public static AsyncServiceScope CreateSystemScope(this IExecutionScopeFactory factory, CallOrigin origin = CallOrigin.Inbox)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return factory.CreateScope(origin, CurrentUser.System());
    }

    /// <summary>从服务容器以指定测试身份创建执行作用域，如 <c>Factory.Services.As(Alice)</c>。</summary>
    public static AsyncServiceScope As(this IServiceProvider services, TestIdentity identity, CallOrigin origin = CallOrigin.Http)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<IExecutionScopeFactory>().CreateScope(identity, origin);
    }

    /// <summary>从服务容器以系统身份创建执行作用域。</summary>
    public static AsyncServiceScope AsSystem(this IServiceProvider services, CallOrigin origin = CallOrigin.Inbox)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<IExecutionScopeFactory>().CreateSystemScope(origin);
    }
}
