using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源授权注册
/// </summary>
internal static class ResourceAuthorizationRegistration
{
    /// <summary>
    /// 注册策略、授权器和可用动作，返回动作目录引用的权限点
    /// </summary>
    public static IReadOnlyList<PermissionReference> AddResourceAuthorization(this IServiceCollection services, IEnumerable<Type> types)
    {
        var candidates = types.Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }).Distinct().ToList();
        var problems = new List<string>();
        var policies = ReadPolicies(candidates, problems);
        var catalogs = ReadCatalogs(candidates, policies, problems);
        if (problems.Count > 0)
        {
            throw new DeclarationException("资源策略或动作目录", problems);
        }

        // 策略按单例注册：误注入数据库上下文这类作用域服务时，构建容器就会报错
        foreach (var ((resource, action), policy) in policies)
        {
            services.AddSingleton(typeof(IResourcePolicy<,>).MakeGenericType(resource, action), policy);
            services.AddScoped(
                typeof(IResourceAuthorizer<,>).MakeGenericType(resource, action),
                typeof(ResourceAuthorizer<,>).MakeGenericType(resource, action));
        }

        var references = new List<PermissionReference>();
        foreach (var (resource, action, catalog) in catalogs)
        {
            services.AddSingleton(catalog.GetType().BaseType!, catalog);
            services.AddScoped(typeof(IAllowedActions<>).MakeGenericType(resource), typeof(AllowedActions<,>).MakeGenericType(resource, action));
            references.AddRange(catalog.Permissions.Select(p => new PermissionReference(p, $"资源动作 {catalog.GetType().Name}")));
        }

        return references;
    }

    private static Dictionary<(Type Resource, Type Action), Type> ReadPolicies(List<Type> candidates, List<string> problems)
    {
        var policies = new Dictionary<(Type Resource, Type Action), Type>();
        foreach (var type in candidates)
        {
            var implemented = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResourcePolicy<,>)).ToList();
            if (implemented.Count > 1)
            {
                problems.Add($"{type.FullName}：一个策略类只能实现一个 IResourcePolicy<,>，每类资源一个策略。");
            }

            foreach (var policy in implemented)
            {
                var key = (policy.GenericTypeArguments[0], policy.GenericTypeArguments[1]);
                if (!policies.TryAdd(key, type))
                {
                    problems.Add($"{type.FullName}：{key.Item1.Name} 已有资源策略 {policies[key].FullName}。");
                }
            }
        }

        return policies;
    }

    private static List<(Type Resource, Type Action, IResourceActionCatalog Catalog)> ReadCatalogs(
        List<Type> candidates,
        Dictionary<(Type Resource, Type Action), Type> policies,
        List<string> problems)
    {
        var catalogs = new List<(Type Resource, Type Action, IResourceActionCatalog Catalog)>();
        foreach (var type in candidates)
        {
            if (type.BaseType is not { IsGenericType: true } baseType || baseType.GetGenericTypeDefinition() != typeof(ResourceActionCatalog<,>))
            {
                continue;
            }

            var (resource, action) = (baseType.GenericTypeArguments[0], baseType.GenericTypeArguments[1]);
            if (!policies.ContainsKey((resource, action)))
            {
                problems.Add($"{type.FullName}：没有 IResourcePolicy<{resource.Name}, {action.Name}> 的实现。");
            }

            if (catalogs.Any(c => c.Resource == resource))
            {
                problems.Add($"{type.FullName}：{resource.Name} 已有动作目录，每类资源只能有一份。");
            }

            if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) is null)
            {
                problems.Add($"{type.FullName}：动作目录只做声明，必须有无参构造函数。");
                continue;
            }

            catalogs.Add((resource, action, (IResourceActionCatalog)Activator.CreateInstance(type, nonPublic: true)!));
        }

        return catalogs;
    }
}
