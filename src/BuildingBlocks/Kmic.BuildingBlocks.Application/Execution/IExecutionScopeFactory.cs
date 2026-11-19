using Kmic.BuildingBlocks.Application.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 执行作用域工厂
/// </summary>
public interface IExecutionScopeFactory
{
    /// <summary>
    /// 新建作用域
    /// </summary>
    /// <remarks>
    /// 作业以用户身份运行时，要先重新加载访问上下文再传入
    /// </remarks>
    AsyncServiceScope CreateScope(CallOrigin origin, ICurrentUser user, IAccessContext? access = null);
}
