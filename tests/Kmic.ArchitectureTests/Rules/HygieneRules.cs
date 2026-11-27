using System.Reflection;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.Extensions.Hosting;

namespace Kmic.ArchitectureTests.Rules;

internal static class HygieneRules
{
    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    // 禁止可变静态字段，避免多请求并发竞态
    public static IReadOnlyList<string> MutableStatics(IEnumerable<Type> types) =>
    [
        .. types.Where(t => !IsCompilerGenerated(t) && IsOurs(t))
            .SelectMany(t => t.GetFields(AllMembers | BindingFlags.FlattenHierarchy).Where(f => f.IsStatic))
            .Where(f => !f.IsInitOnly && !f.IsLiteral && !IsCompilerGenerated(f.DeclaringType!) && f.GetCustomAttribute<ThreadStaticAttribute>() is null)
            .Select(f => $"{f.DeclaringType!.FullName}.{f.Name}：静态字段不能可变，要用 static readonly。"),
    ];

    // 含密码或令牌字段的 record 须自定义 ToString，防止默认实现将敏感信息输出到日志
    public static IReadOnlyList<string> SecretsInToString(IEnumerable<Type> types)
    {
        var secretName = new System.Text.RegularExpressions.Regex("password|refreshtoken|accesstoken|^token$|secret|credential", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        return
        [
            .. types.Where(t => !IsCompilerGenerated(t) && IsOurs(t) && !t.IsAbstract)
                .Where(t => t.GetMethod(nameof(ToString), Type.EmptyTypes) is { } toString
                    && toString.DeclaringType == t
                    && toString.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is not null)
                .Select(t => (Type: t, Secret: t.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(p => p.PropertyType == typeof(string) && secretName.IsMatch(p.Name))))
                .Where(x => x.Secret is not null)
                .Select(x => $"{x.Type.FullName}.{x.Secret!.Name}：带密码或令牌的 record 要自己写 ToString，不能用编译器生成的（会打出所有属性的值）。"),
        ];
    }

    // 业务模块禁止定义异常类型，业务失败统一返回 Result
    public static IReadOnlyList<string> ExceptionTypes(IEnumerable<Type> types) =>
        [.. types.Where(t => typeof(Exception).IsAssignableFrom(t)).Select(t => $"{t.FullName}：模块里不定义异常类型，业务失败返回 Result。")];

    // 模块不直接注册后台服务，后台处理由框架托管
    public static IReadOnlyList<string> HostedServices(IEnumerable<Type> types) =>
        [.. types.Where(t => !t.IsInterface && typeof(IHostedService).IsAssignableFrom(t)).Select(t => $"{t.FullName}：模块不自己实现后台组件，用框架的作业、定时任务和订阅。")];

    // 业务模块禁止用锁或信号量控制并发，应依赖数据库版本号、行锁与唯一约束
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

    // InternalsVisibleTo 仅允许授予本模块的测试项目
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

    // 聚合根之间仅按 ID 引用，且只有聚合根可软删除
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

    // Contracts 不得暴露实体、领域事件与强类型 ID
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

    // 排除源码生成器（如 OpenAPI 的 XML 注释支持）在 Microsoft.* 命名空间下生成的类型
    private static bool IsOurs(Type type) => type.Namespace?.StartsWith("Kmic.", StringComparison.Ordinal) == true;

    private static bool IsCompilerGenerated(Type type) =>
        type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is not null
        || (type.DeclaringType is { } outer && IsCompilerGenerated(outer));
}
