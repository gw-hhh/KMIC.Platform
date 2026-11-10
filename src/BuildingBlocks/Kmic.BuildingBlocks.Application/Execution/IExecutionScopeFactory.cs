using Kmic.BuildingBlocks.Application.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 非 HTTP 入口（Inbox、定时任务、后台作业、启动任务）新建 DI 作用域并设置执行上下文，再经 ISender 发送消息
/// </summary>
public interface IExecutionScopeFactory
{
    /// <summary>
    /// 新建作用域；用户身份的作业要先重新加载提交人的访问上下文再传入
    /// </summary>
    AsyncServiceScope CreateScope(CallOrigin origin, ICurrentUser user, IAccessContext? access = null);
}
