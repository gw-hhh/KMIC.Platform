using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 宿主启动时按顺序执行全部启动任务
/// </summary>
/// <remarks>
/// 在宿主启动阶段执行，失败时抛出，宿主启动失败、进程退出，不会出现“初始化一半就接收请求”
/// </remarks>
internal sealed class StartupTaskRunner(
    IExecutionScopeFactory scopes,
    StartupState state,
    TimeProvider time,
    ILogger<StartupTaskRunner> logger) : IHostedService
{
    private const string FrameworkPrefix = "Framework.";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        List<(string Name, int Order)> tasks;
        await using (var scope = scopes.CreateScope(CallOrigin.Startup, CurrentUser.System()))
        {
            tasks = [.. scope.ServiceProvider.GetServices<IStartupTask>()
                .Select(t => (t.Name, t.Order))
                .OrderBy(t => t.Order)
                .ThenBy(t => t.Name, StringComparer.Ordinal)];
        }

        Validate(tasks);
        foreach (var (name, _) in tasks)
        {
            // 每个任务一个作用域：任务里发送的命令各自独立，前一个任务留下的状态不会影响后一个
            await using var scope = scopes.CreateScope(CallOrigin.Startup, CurrentUser.System());
            var task = scope.ServiceProvider.GetServices<IStartupTask>().Single(t => string.Equals(t.Name, name, StringComparison.Ordinal));
            var started = time.GetTimestamp();
            var result = await task.ExecuteAsync(cancellationToken);
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"启动任务 {name} 失败，拒绝启动：{result.Error.Code} {result.Error.Message}");
            }

            var elapsed = time.GetElapsedTime(started);
            HostingLog.StartupTaskCompleted(logger, name, elapsed.TotalMilliseconds);
        }

        state.Complete();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void Validate(List<(string Name, int Order)> tasks)
    {
        var problems = new List<string>();
        foreach (var duplicate in tasks.GroupBy(t => t.Name, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"启动任务名称“{duplicate.Key}”重复。");
        }

        foreach (var (name, order) in tasks)
        {
            if (name.StartsWith(FrameworkPrefix, StringComparison.Ordinal) != order < 0)
            {
                problems.Add($"启动任务 {name} 的顺序是 {order}：框架任务（Framework. 开头）要小于 0，模块任务要从 0 开始。");
            }
        }

        if (problems.Count > 0)
        {
            throw new DeclarationException("启动任务声明", problems);
        }
    }
}
