using System.Reflection;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.Extensions.Hosting;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 写法上的禁忌：可变静态字段、异常类型、后台组件、并发原语、对测试以外的可见性
/// </summary>
internal static class HygieneRules
{
    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    /// <summary>
    /// 没有可变的静态字段：进程内共享的可变状态在多用户下一定出问题
    /// </summary>
    public static IReadOnlyList<string> MutableStatics(IEnumerable<Type> types) =>
    [
        .. types.Where(t => !IsCompilerGenerated(t) && IsOurs(t))
            .SelectMany(t => t.GetFields(AllMembers | BindingFlags.FlattenHierarchy).Where(f => f.IsStatic))
            .Where(f => !f.IsInitOnly && !f.IsLiteral && !IsCompilerGenerated(f.DeclaringType!) && f.GetCustomAttribute<ThreadStaticAttribute>() is null)
            .Select(f => $"{f.DeclaringType!.FullName}.{f.Name}：静态字段不能可变，要用 static readonly。"),
    ];

    /// <summary>
    /// 模块里不定义异常类型：业务失败用 Result，异常类型只在框架里
    /// </summary>
    public static IReadOnlyList<string> ExceptionTypes(IEnumerable<Type> types) =>
        [.. types.Where(t => typeof(Exception).IsAssignableFrom(t)).Select(t => $"{t.FullName}：模块里不定义异常类型，业务失败返回 Result。")];

    /// <summary>
    /// 模块不注册后台组件：后台处理由框架提供
    /// </summary>
    public static IReadOnlyList<string> HostedServices(IEnumerable<Type> types) =>
        [.. types.Where(t => !t.IsInterface && typeof(IHostedService).IsAssignableFrom(t)).Select(t => $"{t.FullName}：模块不自己实现后台组件，用框架的作业、定时任务和订阅。")];

    /// <summary>
    /// 模块里不用锁和信号量处理业务并发：业务并发靠数据库的版本、行锁和唯一约束
    /// </summary>
    public static IReadOnlyList<string> ConcurrencyPrimitives(IEnumerable<Type> types)
    {
        var forbidden = new[] { typeof(SemaphoreSlim), typeof(Mutex), typeof(Lock), typeof(ReaderWriterLockSlim) };
        return
        [
            .. types.Where(t => !IsCompilerGenerated(t) && IsOurs(t))
                .SelectMany(t => t.GetFields(AllMembers).Select(f => (Type: t, f.Name, f.FieldType)))
                .Where(x => forbidden.Contains(x.FieldType))
                .Select(x => $"{x.Type.FullName}.{x.Name}：模块里不用 {x.FieldType.Name} 处理业务并发，用数据库的版本、行锁和唯一约束。"),
        ];
    }

    /// <summary>
    /// InternalsVisibleTo 只给本模块自己的测试项目
    /// </summary>
    public static IReadOnlyList<string> VisibleTo(Assembly assembly, string module)
    {
        var allowed = new System.Text.RegularExpressions.Regex($@"^Kmic\.{module}\.[A-Za-z.]*Tests$", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        return
        [
            .. assembly.GetCustomAttributes<System.Runtime.CompilerServices.InternalsVisibleToAttribute>()
                .Where(a => !allowed.IsMatch(a.AssemblyName))
                .Select(a => $"{assembly.GetName().Name}：InternalsVisibleTo 指向了 {a.AssemblyName}，只能指向本模块自己的测试项目。"),
        ];
    }

    /// <summary>
    /// 聚合根的属性不能是另一个聚合根，聚合之间只按 ID 引用；只有聚合根可以软删除
    /// </summary>
    public static IReadOnlyList<string> Aggregates(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(t => t is { IsClass: true } && typeof(IAggregateRoot).IsAssignableFrom(t)))
        {
            problems.AddRange(type.GetProperties(AllMembers)
                .Where(p => IsOrContainsAggregate(p.PropertyType))
                .Select(p => $"{type.FullName}.{p.Name}：聚合根不持有另一个聚合根，按 ID 引用。"));
        }

        problems.AddRange(types
            .Where(t => t is { IsClass: true } && typeof(ISoftDeletable).IsAssignableFrom(t) && !typeof(IAggregateRoot).IsAssignableFrom(t))
            .Select(t => $"{t.FullName}：只有聚合根可以软删除。"));
        return problems;
    }

    /// <summary>
    /// Contracts 里没有领域类型：实体、领域事件、强类型 ID 都不出模块
    /// </summary>
    public static IReadOnlyList<string> ContractsContent(IEnumerable<Type> types) =>
    [
        .. types.Where(t => IsEntity(t) || typeof(IDomainEvent).IsAssignableFrom(t) || typeof(IStronglyTypedId).IsAssignableFrom(t))
            .Select(t => $"{t.FullName}：Contracts 里不放实体、领域事件和强类型 ID，跨模块用 Ref 和集成事件。"),
    ];

    private static bool IsEntity(Type type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsOrContainsAggregate(Type type) =>
        typeof(IAggregateRoot).IsAssignableFrom(type)
        || type.GetInterfaces().Concat([type]).Where(i => i.IsGenericType).SelectMany(i => i.GetGenericArguments()).Any(a => typeof(IAggregateRoot).IsAssignableFrom(a));

    // 源码生成器（如 OpenAPI 的 XML 注释支持）放在 Microsoft.* 命名空间的生成代码不归我们管
    private static bool IsOurs(Type type) => type.Namespace?.StartsWith("Kmic.", StringComparison.Ordinal) == true;

    private static bool IsCompilerGenerated(Type type) =>
        type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is not null
        || (type.DeclaringType is { } outer && IsCompilerGenerated(outer));
}
