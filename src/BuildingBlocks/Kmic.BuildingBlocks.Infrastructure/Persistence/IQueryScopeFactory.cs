using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在单例服务里使用模块查询上下文的入口
/// </summary>
/// <remarks>
/// 每次调用新开一个系统身份的作用域
/// </remarks>
public interface IQueryScopeFactory
{
    /// <summary>
    /// 在新的作用域里取模块的查询上下文并执行
    /// </summary>
    /// <typeparam name="TContext">模块的查询上下文类型</typeparam>
    /// <typeparam name="TResult">结果类型，不能把上下文或延迟执行的查询带出去</typeparam>
    Task<TResult> RunAsync<TContext, TResult>(CallOrigin origin, Func<TContext, CancellationToken, Task<TResult>> work, CancellationToken ct)
        where TContext : ModuleQueryDbContext;
}
