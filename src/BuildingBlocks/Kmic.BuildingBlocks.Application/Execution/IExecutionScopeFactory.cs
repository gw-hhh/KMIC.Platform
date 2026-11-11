using Kmic.BuildingBlocks.Application.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 执行作用域工厂
/// </summary>
/// <remarks>
/// Inbox、定时任务、后台作业、启动任务用它新建作用域，再通过 ISender 发消息
/// </remarks>
public interface IExecutionScopeFactory
{
    /// <summary>
    /// 新建作用域
    /// </summary>
    /// <remarks>
    /// 以用户身份运行的作业，要先重新加载提交人的访问上下文再传进来
    /// </remarks>
    AsyncServiceScope CreateScope(CallOrigin origin, ICurrentUser user, IAccessContext? access = null);
}
