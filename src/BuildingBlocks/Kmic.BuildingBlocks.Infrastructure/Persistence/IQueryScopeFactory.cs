using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>在单例服务中使用模块查询上下文的工厂（每次调用创建独立作用域并以系统身份执行）</summary>
public interface IQueryScopeFactory
{
    /// <summary>在新建作用域中获取指定模块的查询上下文并执行查询（禁止将上下文或延迟查询传出作用域）</summary>
    Task<TResult> RunAsync<TContext, TResult>(CallOrigin origin, Func<TContext, CancellationToken, Task<TResult>> work, CancellationToken ct)
        where TContext : ModuleQueryDbContext;
}
