using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 在单例服务里使用模块查询上下文的入口
/// </summary>
/// <remarks>
/// 查询上下文是作用域服务，单例（如访问上下文加载器）不能直接注入。每次调用新开一个系统身份的作用域，
/// 用完即释放。模块代码不能自己从 IServiceProvider 取服务（禁用 API），所以由框架提供
/// </remarks>
public interface IQueryScopeFactory
{
    /// <summary>
    /// 在新的作用域里取模块的查询上下文并执行
    /// </summary>
    /// <param name="origin">入口来源，决定用前台读池还是后台读池</param>
    /// <param name="work">要执行的查询，上下文只在它执行期间有效</param>
    /// <param name="ct">取消令牌</param>
    /// <typeparam name="TContext">模块的查询上下文类型</typeparam>
    /// <typeparam name="TResult">结果类型，不能把上下文或延迟执行的查询带出去</typeparam>
    Task<TResult> RunAsync<TContext, TResult>(CallOrigin origin, Func<TContext, CancellationToken, Task<TResult>> work, CancellationToken ct)
        where TContext : ModuleQueryDbContext;
}
