using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Scheduling;

public sealed class ScheduledJobCatalog
{
    private readonly FrozenDictionary<string, ScheduledJobDefinition> _byName;

    private ScheduledJobCatalog(IReadOnlyList<ScheduledJobDefinition> all)
    {
        All = all;
        _byName = all.ToFrozenDictionary(j => j.Name, StringComparer.Ordinal);
    }

    public IReadOnlyList<ScheduledJobDefinition> All { get; }

    /// <summary>按名称查找定时任务定义，未声明时返回 null。</summary>
    public ScheduledJobDefinition? Find(string name) => _byName.GetValueOrDefault(name);

    internal static ScheduledJobCatalog Build(IEnumerable<ModuleSchedules> modules, UseCaseCatalog useCases, Func<string, string?> checkCron)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(useCases);
        ArgumentNullException.ThrowIfNull(checkCron);

        var problems = new List<string>();
        var all = modules.SelectMany(m => m.Schedules.Select(s => Read(m.Module, s, useCases, checkCron, problems))).ToList();
        foreach (var duplicate in all.GroupBy(j => j.Name, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"定时任务名“{duplicate.Key}”声明了 {duplicate.Count()} 次。");
        }

        return problems.Count > 0 ? throw new DeclarationException("定时任务声明", problems) : new ScheduledJobCatalog(all);
    }

    private static ScheduledJobDefinition Read(string module, DeclaredSchedule declared, UseCaseCatalog useCases, Func<string, string?> checkCron, List<string> problems)
    {
        var name = declared.Name;
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"定时任务名“{name}”须以“{module}.”开头。");
        }

        var useCase = useCases.Find(declared.CommandType);
        if (useCase is null || useCase.Kind != MessageKind.Command || !string.Equals(useCase.Module, module, StringComparison.Ordinal))
        {
            problems.Add($"定时任务 {name} 发送的 {declared.CommandType.Name} 要是 {module} 模块的命令。");
        }
        else if (useCase.Requirement != AccessRequirement.SystemOnly)
        {
            problems.Add($"定时任务 {name} 发送的 {declared.CommandType.Name} 仅限系统执行，须标注 [SystemOnly]。");
        }

        if (checkCron(declared.Cron) is { } cronProblem)
        {
            problems.Add($"定时任务 {name} 的 Cron 表达式“{declared.Cron}”无效：{cronProblem}");
        }

        return new ScheduledJobDefinition(
            name, module, declared.CommandType, declared.Cron, declared.Create, declared.Options.Misfire, declared.Options.Display ?? name);
    }
}
