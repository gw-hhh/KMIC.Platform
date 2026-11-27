using System.Reflection;
using Kmic.ArchitectureTests.Support;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.ArchitectureTests.Rules;

internal static class CapabilityRules
{
    private const string IdentityModule = "Identity";
    private const string FilesModule = "Files";

    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    // [AllowAnonymous] 仅限 Identity 模块使用
    public static IReadOnlyList<string> AnonymousAccess(IEnumerable<ProductAssembly> assemblies) =>
    [
        .. assemblies.Where(a => a.IsModule && !string.Equals(a.Module, IdentityModule, StringComparison.Ordinal))
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => t.IsDefined(typeof(AllowAnonymousAttribute), inherit: false))
            .Select(t => $"{t.FullName}：只有 Identity 模块的用例可以声明 [AllowAnonymous]。"),
    ];

    // 仅 Files 模块与框架可使用 IFileStorage，其他模块应通过 Files 契约接口存取文件
    public static IReadOnlyList<string> FileStorage(IEnumerable<ProductAssembly> assemblies) =>
    [
        .. assemblies.Where(a => a.IsModule && !string.Equals(a.Module, FilesModule, StringComparison.Ordinal))
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => MemberTypes(t).Any(m => m == typeof(IFileStorage)))
            .Select(t => $"{t.FullName}：只有 Files 模块能使用 IFileStorage。"),
    ];

    // HybridCache 仅限 Infrastructure 与查询处理器使用，命令不读缓存
    public static IReadOnlyList<string> Cache(IEnumerable<ProductAssembly> assemblies) =>
    [
        .. assemblies.Where(a => a.IsModule && a.Layer != Layer.Infrastructure)
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => !IsQueryHandler(t) && MemberTypes(t).Any(m => m == typeof(HybridCache)))
            .Select(t => $"{t.FullName}：HybridCache 只能在 Infrastructure 和查询处理器中使用。"),
    ];

    private static bool IsQueryHandler(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

    // 提取类型自身声明的字段、属性、构造函数及方法的参数与返回值类型
    private static IEnumerable<Type> MemberTypes(Type type) =>
        type.GetFields(AllMembers).Select(f => f.FieldType)
            .Concat(type.GetProperties(AllMembers).Select(p => p.PropertyType))
            .Concat(type.GetConstructors(AllMembers).SelectMany(c => c.GetParameters().Select(p => p.ParameterType)))
            .Concat(type.GetMethods(AllMembers).SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType)));
}
