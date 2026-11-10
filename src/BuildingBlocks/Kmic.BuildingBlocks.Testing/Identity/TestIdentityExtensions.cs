using Kmic.BuildingBlocks.Application.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 以测试身份建立执行作用域
/// </summary>
public static class TestIdentityExtensions
{
    /// <summary>
    /// 以测试身份建立作用域，来源默认为 HTTP；在作用域中发送的消息走完整管道
    /// </summary>
    public static AsyncServiceScope CreateScope(this IExecutionScopeFactory factory, TestIdentity identity, CallOrigin origin = CallOrigin.Http)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(identity);
        return factory.CreateScope(origin, CurrentUser.From(identity), identity);
    }

    /// <summary>
    /// 以系统身份建立作用域，如 Inbox、定时任务
    /// </summary>
    public static AsyncServiceScope CreateSystemScope(this IExecutionScopeFactory factory, CallOrigin origin = CallOrigin.Inbox)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return factory.CreateScope(origin, CurrentUser.System());
    }
}
