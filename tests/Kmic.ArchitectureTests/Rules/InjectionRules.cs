using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Kmic.ArchitectureTests.Rules;

internal static class InjectionRules
{
    private const BindingFlags AllMethods = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    // 命令处理器不能注入命令发送器，复用逻辑应下沉到领域服务
    public static IReadOnlyList<string> CommandHandlers(IEnumerable<Type> types) =>
    [
        .. types.Where(IsCommandHandler)
            .SelectMany(t => ConstructorTypes(t).Where(p => p == typeof(ICommandSender) || p == typeof(ISender)).Select(p => $"{t.FullName}：命令处理器不能注入 {p.Name}，命令里不能再发命令。")),
    ];

    // 校验器只校验输入本身，不得注入 DbContext、仓储或查询入口
    public static IReadOnlyList<string> Validators(IEnumerable<Type> types) =>
    [
        .. types.Where(IsValidator)
            .SelectMany(t => ConstructorTypes(t).Where(IsDataAccess).Select(p => $"{t.FullName}：校验器不能注入 {p.Name}，要查数据库的判断放在处理器里。")),
    ];

    // 命令处理器不得注入查询上下文（读不到未提交变更），读写数据一律走仓储
    public static IReadOnlyList<string> CommandHandlersAndQueryContexts(IEnumerable<Type> types) =>
    [
        .. types.Where(IsCommandHandler)
            .SelectMany(t => ConstructorTypes(t).Where(IsQueryContext).Select(p => $"{t.FullName}：命令处理器不能注入 {p.Name}，要读的数据经仓储取。")),
    ];

    // 后台作业处理器只做命令/查询编排，不直接注入 DbContext、仓储或查询入口
    public static IReadOnlyList<string> JobHandlers(IEnumerable<Type> types) =>
    [
        .. types.Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJobHandler<>)))
            .SelectMany(t => ConstructorTypes(t).Where(IsDataAccess).Select(p => $"{t.FullName}：作业处理器不能注入 {p.Name}，通过命令和查询访问数据。")),
    ];

    // 应用层不得注入非缓存的 IFreshSettings，该接口仅限基础设施层后台任务使用
    public static IReadOnlyList<string> FreshSettings(IEnumerable<Type> types) =>
    [
        .. types.Where(t => t is { IsClass: true })
            .SelectMany(t => ConstructorTypes(t).Where(p => p == typeof(IFreshSettings)).Select(p => $"{t.FullName}：应用层不能注入 {p.Name}，读设置用 ISettings（经缓存）；每次读库的读取只给基础设施里的后台判断用。")),
    ];

    // 授权组件不得依赖用户偏好，避免用户自行修改偏好影响授权判定
    public static IReadOnlyList<string> AuthorizationCode(IEnumerable<Type> types) =>
    [
        .. types.Where(IsAuthorizationCode)
            .SelectMany(t => ConstructorTypes(t).Where(p => p == typeof(IPreferences)).Select(p => $"{t.FullName}：授权相关的代码不能依赖 {p.Name}。")),
    ];

    // 端点只能通过发送器调用用例，不得直接依赖仓储、DbContext 或查询入口
    public static IReadOnlyList<string> Endpoints(IEnumerable<Type> types) =>
    [
        .. types.Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IEndpoint).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods(AllMethods).Concat<MethodBase>(t.GetConstructors(AllMethods))
                .SelectMany(m => m.GetParameters().Select(p => p.ParameterType))
                .Where(IsDataAccess)
                .Select(p => $"{t.FullName}：端点不能依赖 {p.Name}，只通过 ICommandSender、IQuerySender 发消息。")),
    ];

    private static bool IsCommandHandler(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) || i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)));

    private static bool IsValidator(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Type> ConstructorTypes(Type type) =>
        type.GetConstructors(AllMethods).SelectMany(c => c.GetParameters().Select(p => p.ParameterType));

    // 查询上下文：模块的查询 DbContext，以及它对外的查询入口（名字以 QueryDb 结尾的接口）
    private static bool IsQueryContext(Type type) =>
        typeof(ModuleQueryDbContext).IsAssignableFrom(type) || (type.IsInterface && type.Name.EndsWith("QueryDb", StringComparison.Ordinal));

    private static bool IsAuthorizationCode(Type type)
    {
        if (type is not { IsClass: true, IsAbstract: false })
        {
            return false;
        }

        return typeof(IAccessContextLoader).IsAssignableFrom(type)
            || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResourcePolicy<,>))
            || IsResourceActionCatalog(type);
    }

    private static bool IsResourceActionCatalog(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ResourceActionCatalog<,>))
            {
                return true;
            }
        }

        return false;
    }

    // DbContext、仓储，以及模块的查询入口（名字以 QueryDb 结尾的接口）
    private static bool IsDataAccess(Type type) =>
        typeof(DbContext).IsAssignableFrom(type)
        || type.GetInterfaces().Concat([type]).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>))
        || (type.IsInterface && type.Name.EndsWith("QueryDb", StringComparison.Ordinal));
}
