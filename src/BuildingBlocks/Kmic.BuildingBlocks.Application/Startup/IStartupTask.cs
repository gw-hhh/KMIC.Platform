using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Startup;

/// <summary>
/// 启动任务
/// </summary>
/// <remarks>
/// 每个实例启动时按顺序执行，全部成功后才接收请求、开始后台处理；任一失败进程就退出。
/// 多个实例会同时执行，任务自己要幂等，并用 pg_advisory_xact_lock 保证只有一个实例真正写入
/// </remarks>
public interface IStartupTask
{
    /// <summary>
    /// 稳定名称，如 Identity.SyncPermissionCatalog；框架任务以 Framework. 开头
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 执行顺序，框架任务小于 0，模块任务从 0 开始
    /// </summary>
    int Order { get; }

    /// <summary>
    /// 执行，通常是发送一个 [SystemOnly] 命令
    /// </summary>
    ValueTask<Result> ExecuteAsync(CancellationToken ct);
}
