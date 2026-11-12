using System.Reflection;
using Kmic.ArchitectureTests.Support;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Microsoft.Extensions.Caching.Hybrid;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 只有指定的模块或层才能用的能力：匿名访问、文件存储、缓存
/// </summary>
internal static class CapabilityRules
{
    private const string IdentityModule = "Identity";
    private const string FilesModule = "Files";

    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    /// <summary>
    /// [AllowAnonymous] 只能出现在 Identity 模块：登录、刷新令牌这类入口之外，所有用例都要登录
    /// </summary>
    public static IReadOnlyList<string> AnonymousAccess(IEnumerable<ProductAssembly> assemblies) =>
    [
        .. assemblies.Where(a => a.IsModule && !string.Equals(a.Module, IdentityModule, StringComparison.Ordinal))
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => t.IsDefined(typeof(AllowAnonymousAttribute), inherit: false))
            .Select(t => $"{t.FullName}：只有 Identity 模块的用例可以声明 [AllowAnonymous]。"),
    ];

    /// <summary>
    /// 文件存储：只有 Files 模块和框架能用 IFileStorage，其他模块通过 Files 模块的接口存取文件
    /// </summary>
    public static IReadOnlyList<string> FileStorage(IEnumerable<ProductAssembly> assemblies) =>
    [
        .. assemblies.Where(a => a.IsModule && !string.Equals(a.Module, FilesModule, StringComparison.Ordinal))
            .SelectMany(a => a.Assembly.GetTypes())
            .Where(t => MemberTypes(t).Any(m => m == typeof(IFileStorage)))
            .Select(t => $"{t.FullName}：只有 Files 模块能使用 IFileStorage。"),
    ];

    /// <summary>
    /// HybridCache 只在 Infrastructure 和查询处理器中使用：命令不读缓存，领域、契约、接口层不碰缓存
    /// </summary>
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

    // 类型自己声明的字段、属性、构造函数和方法的参数与返回值类型
    private static IEnumerable<Type> MemberTypes(Type type) =>
        type.GetFields(AllMembers).Select(f => f.FieldType)
            .Concat(type.GetProperties(AllMembers).Select(p => p.PropertyType))
            .Concat(type.GetConstructors(AllMembers).SelectMany(c => c.GetParameters().Select(p => p.ParameterType)))
            .Concat(type.GetMethods(AllMembers).SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType)));
}
