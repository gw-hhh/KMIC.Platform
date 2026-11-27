using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Startup;

/// <summary>实例启动时按顺序执行的启动任务；多实例并发启动时会同时执行，实现须保证幂等。</summary>
public interface IStartupTask
{
    /// <summary>稳定名称（如 Identity.SyncPermissionCatalog），框架任务以 Framework. 开头。</summary>
    string Name { get; }

    /// <summary>执行顺序；框架任务小于 0，模块任务从 0 开始。</summary>
    int Order { get; }

    /// <summary>执行启动任务，通常为发送一条 [SystemOnly] 命令。</summary>
    ValueTask<Result> ExecuteAsync(CancellationToken ct);
}
