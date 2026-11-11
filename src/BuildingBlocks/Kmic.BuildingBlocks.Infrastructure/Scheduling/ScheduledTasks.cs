using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.Scheduling;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 本实例声明的全部定时任务：模块的加上框架的
/// </summary>
internal sealed class ScheduledTasks
{
    private const string FrameworkPrefix = "Framework.";

    private readonly ScheduledJobCatalog _catalog;
    private readonly FrozenDictionary<string, IFrameworkTask> _framework;

    public ScheduledTasks(ScheduledJobCatalog catalog, IEnumerable<IFrameworkTask> framework)
    {
        _catalog = catalog;
        var tasks = framework.ToList();
        var problems = tasks.Where(t => !t.Name.StartsWith(FrameworkPrefix, StringComparison.Ordinal)).Select(t => $"框架任务名“{t.Name}”要以“{FrameworkPrefix}”开头。")
            .Concat(catalog.All.Where(j => j.Name.StartsWith(FrameworkPrefix, StringComparison.Ordinal)).Select(j => $"模块的定时任务不能用“{FrameworkPrefix}”开头：{j.Name}。"))
            .Concat(tasks.GroupBy(t => t.Name, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => $"框架任务“{g.Key}”注册了 {g.Count()} 次。"))
            .ToList();
        if (problems.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, problems));
        }

        _framework = tasks.ToFrozenDictionary(t => t.Name, StringComparer.Ordinal);
        All =
        [
            .. catalog.All.Select(j => new ScheduleEntry(j.Name, j.Cron, j.Misfire, j.DisplayName)),
            .. tasks.Select(t => new ScheduleEntry(t.Name, t.Cron, Misfire.FireOnceNow, t.DisplayName)),
        ];
    }

    public IReadOnlyList<ScheduleEntry> All { get; }

    public ScheduledJobDefinition? FindModuleJob(string name) => _catalog.Find(name);

    public IFrameworkTask? FindFrameworkTask(string name) => _framework.GetValueOrDefault(name);

    public bool Contains(string name) => FindModuleJob(name) is not null || _framework.ContainsKey(name);
}
