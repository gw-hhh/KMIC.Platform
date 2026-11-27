using Kmic.BuildingBlocks.Application.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Execution;

public interface IExecutionScopeFactory
{
    /// <summary>新建执行作用域；作业以用户身份执行时，须先重新加载访问上下文再传入。</summary>
    AsyncServiceScope CreateScope(CallOrigin origin, ICurrentUser user, IAccessContext? access = null);
}
