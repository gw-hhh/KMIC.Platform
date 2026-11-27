using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

// 宿主启动时按 Order 顺序执行全部启动任务
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
            // 每个启动任务使用独立作用域，避免命令执行状态在任务间相互污染
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
                problems.Add($"启动任务 {name} 的执行顺序（{order}）无效：框架任务（以 Framework. 开头）须小于 0，模块任务须大于或等于 0。");
            }
        }

        if (problems.Count > 0)
        {
            throw new DeclarationException("启动任务声明", problems);
        }
    }
}
