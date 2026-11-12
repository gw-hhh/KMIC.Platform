using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 依赖注入：每一类构件允许依赖什么
/// </summary>
internal static class InjectionRules
{
    private const BindingFlags AllMethods = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    /// <summary>
    /// 命令处理器不能注入命令发送器：命令里不能再发命令，复用的逻辑放进领域服务
    /// </summary>
    public static IReadOnlyList<string> CommandHandlers(IEnumerable<Type> types) =>
    [
        .. types.Where(IsCommandHandler)
            .SelectMany(t => ConstructorTypes(t).Where(p => p == typeof(ICommandSender) || p == typeof(ISender)).Select(p => $"{t.FullName}：命令处理器不能注入 {p.Name}，命令里不能再发命令。")),
    ];

    /// <summary>
    /// 校验器只看命令本身：不注入 DbContext、仓储、查询入口，数据库相关的判断放在处理器里
    /// </summary>
    public static IReadOnlyList<string> Validators(IEnumerable<Type> types) =>
    [
        .. types.Where(IsValidator)
            .SelectMany(t => ConstructorTypes(t).Where(IsDataAccess).Select(p => $"{t.FullName}：校验器不能注入 {p.Name}，要查数据库的判断放在处理器里。")),
    ];

    /// <summary>
    /// 端点只注入命令和查询发送器：不碰仓储、DbContext、查询入口
    /// </summary>
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

    // DbContext、仓储，以及模块的查询入口（名字以 QueryDb 结尾的接口）
    private static bool IsDataAccess(Type type) =>
        typeof(DbContext).IsAssignableFrom(type)
        || type.GetInterfaces().Concat([type]).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>))
        || (type.IsInterface && type.Name.EndsWith("QueryDb", StringComparison.Ordinal));
}
