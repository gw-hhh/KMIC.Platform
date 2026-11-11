using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业目录
/// </summary>
public sealed class BackgroundJobCatalog
{
    private readonly FrozenDictionary<Type, BackgroundJobType> _byType;
    private readonly FrozenDictionary<(string Name, int Version), BackgroundJobType> _byName;

    private BackgroundJobCatalog(IReadOnlyCollection<BackgroundJobType> jobs)
    {
        _byType = jobs.ToFrozenDictionary(j => j.ClrType);
        _byName = jobs.ToFrozenDictionary(j => (j.Name, j.Version));
    }

    /// <summary>
    /// 没有任何作业的目录
    /// </summary>
    public static BackgroundJobCatalog Empty { get; } = new([]);

    /// <summary>
    /// 作业声明里引用的权限点，交给权限点目录核对
    /// </summary>
    public IEnumerable<PermissionReference> PermissionReferences =>
        _byType.Values.Where(j => j.Attribute.Permission.Length > 0).Select(j => new PermissionReference(j.Attribute.Permission, $"后台作业 {j.Name}"));

    internal IEnumerable<BackgroundJobType> All => _byType.Values;

    /// <summary>
    /// 从各模块的 Application 程序集中读取作业和处理器
    /// </summary>
    public static BackgroundJobCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Append(typeof(ReadModelRebuildJob)));
    }

    internal static BackgroundJobCatalog Build(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var all = types.Distinct().Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }).ToList();
        var handlers = all
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobHandler<>))
                .Select(i => (Job: i.GetGenericArguments()[0], Handler: t)))
            .ToLookup(h => h.Job, h => h.Handler);

        var problems = new List<string>();
        var jobs = all.Where(t => t.IsAssignableTo(typeof(IBackgroundJob)))
            .Select(t => Read(t, handlers[t].ToList(), problems))
            .OfType<BackgroundJobType>()
            .ToList();
        foreach (var duplicate in jobs.GroupBy(j => (j.Name, j.Version)).Where(g => g.Count() > 1))
        {
            problems.Add($"作业名称“{duplicate.Key.Name}”版本 {duplicate.Key.Version} 重复：{string.Join("、", duplicate.Select(j => j.ClrType.FullName))}。");
        }

        return problems.Count > 0 ? throw new DeclarationException("后台作业声明", problems) : new BackgroundJobCatalog(jobs);
    }

    internal BackgroundJobType Describe(Type jobType) =>
        _byType.GetValueOrDefault(jobType)
            ?? throw new InvalidOperationException($"{jobType.FullName} 不在后台作业目录中，检查它是否标了 [BackgroundJob]、所在模块是否已注册。");

    internal BackgroundJobType? Find(string name, int version) => _byName.GetValueOrDefault((name, version));

    private static BackgroundJobType? Read(Type type, List<Type> handlers, List<string> problems)
    {
        if (type.GetCustomAttribute<BackgroundJobAttribute>() is not { } attribute)
        {
            problems.Add($"{type.FullName} 实现了 IBackgroundJob，但没有标 [BackgroundJob]。");
            return null;
        }

        var name = attribute.Name;
        var module = ModuleNames.Of(type);
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"{type.Name} 的作业名称“{name}”要以“{module}.”开头。");
        }

        if (attribute.Version < 1 || attribute.MaxConcurrentPerUser < 1)
        {
            problems.Add($"作业 {name} 的版本号和每人同时运行的上限都要从 1 开始。");
        }

        if ((attribute.Permission.Length > 0) == attribute.SystemOnly)
        {
            problems.Add($"作业 {name} 要么写 Permission（用户提交），要么写 SystemOnly = true（只能系统提交），只能选一个。");
        }

        // 框架自己的作业，处理器在基础设施层，由那边注册
        if (type.Assembly == typeof(BackgroundJobCatalog).Assembly)
        {
            return new BackgroundJobType(name, attribute.Version, type, module, null, attribute);
        }

        if (handlers.Count != 1)
        {
            problems.Add($"作业 {name} 要有且只有一个处理器，现在有 {handlers.Count} 个。");
            return null;
        }

        if (!string.Equals(ModuleNames.Of(handlers[0]), module, StringComparison.Ordinal))
        {
            problems.Add($"作业 {name} 的处理器 {handlers[0].Name} 要在 {module} 模块里。");
        }

        return new BackgroundJobType(name, attribute.Version, type, module, handlers[0], attribute);
    }
}
