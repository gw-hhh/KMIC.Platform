using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Application.Jobs;

public sealed class BackgroundJobCatalog
{
    private readonly FrozenDictionary<Type, BackgroundJobType> _byType;
    private readonly FrozenDictionary<(string Name, int Version), BackgroundJobType> _byName;
    private readonly FrozenDictionary<string, string> _displayNames;

    private BackgroundJobCatalog(IReadOnlyCollection<BackgroundJobType> jobs)
    {
        _byType = jobs.ToFrozenDictionary(j => j.ClrType);
        _byName = jobs.ToFrozenDictionary(j => (j.Name, j.Version));
        _displayNames = jobs.GroupBy(j => j.Name, StringComparer.Ordinal)
            .ToFrozenDictionary(g => g.Key, g => g.MaxBy(j => j.Version)!.Attribute.DisplayName, StringComparer.Ordinal);
    }

    public static BackgroundJobCatalog Empty { get; } = new([]);

    /// <summary>作业声明引用的权限点，供权限点目录校验。</summary>
    public IEnumerable<PermissionReference> PermissionReferences =>
        _byType.Values.Where(j => j.Attribute.Permission.Length > 0).Select(j => new PermissionReference(j.Attribute.Permission, $"后台作业 {j.Name}"));

    internal IEnumerable<BackgroundJobType> All => _byType.Values;

    /// <summary>从各模块 Application 程序集加载作业与处理器声明。</summary>
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
            ?? throw new InvalidOperationException($"{jobType.FullName} 不在后台作业目录中，请检查其是否已标注 [BackgroundJob] 且所属模块已注册。");

    internal BackgroundJobType? Find(string name, int version) => _byName.GetValueOrDefault((name, version));

    /// <summary>按作业类型名获取显示名称；未定义该作业时原样返回类型名。</summary>
    public string DisplayNameOf(string jobType)
    {
        ArgumentNullException.ThrowIfNull(jobType);
        return _displayNames.GetValueOrDefault(jobType) ?? jobType;
    }

    private static BackgroundJobType? Read(Type type, List<Type> handlers, List<string> problems)
    {
        if (type.GetCustomAttribute<BackgroundJobAttribute>() is not { } attribute)
        {
            problems.Add($"{type.FullName} 实现了 IBackgroundJob，但未标注 [BackgroundJob]。");
            return null;
        }

        var name = attribute.Name;
        var module = ModuleNames.Of(type);
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"{type.Name} 的作业名称“{name}”须以“{module}.”开头。");
        }

        if (string.IsNullOrWhiteSpace(attribute.DisplayName))
        {
            problems.Add($"作业 {name} 须指定显示名称。");
        }

        if (!attribute.Audited && name.Contains("Export", StringComparison.Ordinal))
        {
            problems.Add($"作业 {name} 为导出作业，必须声明 Audited = true 以保留导出审计记录。");
        }

        if (attribute.Version < 1 || attribute.MaxConcurrentPerUser < 1)
        {
            problems.Add($"作业 {name} 的版本号与单用户并发上限均须大于等于 1。");
        }

        if ((attribute.Permission.Length > 0) == attribute.SystemOnly)
        {
            problems.Add($"作业 {name} 须指定 Permission（用户提交）或设置 SystemOnly = true（仅系统提交），两者只能选一个。");
        }

        // 框架内置作业的处理器位于基础设施层，由基础设施层负责注册
        if (type.Assembly == typeof(BackgroundJobCatalog).Assembly)
        {
            return new BackgroundJobType(name, attribute.Version, type, module, null, attribute);
        }

        if (handlers.Count != 1)
        {
            problems.Add($"作业 {name} 必须有且只有一个处理器，当前为 {handlers.Count} 个。");
            return null;
        }

        if (!string.Equals(ModuleNames.Of(handlers[0]), module, StringComparison.Ordinal))
        {
            problems.Add($"作业 {name} 的处理器 {handlers[0].Name} 须位于 {module} 模块中。");
        }

        return new BackgroundJobType(name, attribute.Version, type, module, handlers[0], attribute);
    }
}
