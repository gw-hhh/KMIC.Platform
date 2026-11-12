using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 命名与形态：命令、查询、处理器、校验器、端点、领域事件、强类型 ID、错误目录
/// </summary>
internal static class NamingRules
{
    /// <summary>
    /// 命令以 Command 结尾，查询以 Query 结尾，都是 sealed record
    /// </summary>
    public static IReadOnlyList<string> Messages(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(IsConcrete))
        {
            var suffix = MessageSuffix(type);
            if (suffix is null)
            {
                continue;
            }

            if (!type.Name.EndsWith(suffix, StringComparison.Ordinal))
            {
                problems.Add($"{type.FullName}：{(string.Equals(suffix, "Command", StringComparison.Ordinal) ? "命令" : "查询")}的名字要以 {suffix} 结尾。");
            }

            if (!type.IsSealed || !IsRecord(type))
            {
                problems.Add($"{type.FullName}：消息要写成 sealed record。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 命令、查询处理器是 public sealed，名字以 Handler 结尾；Mediator 在宿主里生成的分发代码要直接引用它们，所以不能是 internal
    /// </summary>
    public static IReadOnlyList<string> Handlers(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(t => IsConcrete(t) && IsHandler(t)))
        {
            if (!(type.IsPublic || type.IsNestedPublic) || !type.IsSealed)
            {
                problems.Add($"{type.FullName}：处理器要写成 public sealed。");
            }

            if (!type.Name.EndsWith("Handler", StringComparison.Ordinal))
            {
                problems.Add($"{type.FullName}：处理器的名字要以 Handler 结尾。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 校验器、端点是 internal sealed，名字分别以 Validator、Endpoint 结尾
    /// </summary>
    public static IReadOnlyList<string> InternalParts(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(IsConcrete))
        {
            var kind = PartKind(type);
            if (kind is null)
            {
                continue;
            }

            if (type.IsPublic || type.IsNestedPublic || !type.IsSealed)
            {
                problems.Add($"{type.FullName}：{(string.Equals(kind, "Validator", StringComparison.Ordinal) ? "校验器" : "端点")}要写成 internal sealed。");
            }

            if (!type.Name.EndsWith(kind, StringComparison.Ordinal))
            {
                problems.Add($"{type.FullName}：{(string.Equals(kind, "Validator", StringComparison.Ordinal) ? "校验器" : "端点")}的名字要以 {kind} 结尾。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 领域事件是 sealed record
    /// </summary>
    public static IReadOnlyList<string> DomainEvents(IEnumerable<Type> types) =>
    [
        .. types.Where(t => IsConcrete(t) && typeof(IDomainEvent).IsAssignableFrom(t) && (!t.IsSealed || !IsRecord(t)))
            .Select(t => $"{t.FullName}：领域事件要写成 sealed record。"),
    ];

    /// <summary>
    /// 强类型 ID 的名字以 Id 或 Ref 结尾
    /// </summary>
    public static IReadOnlyList<string> Identifiers(IEnumerable<Type> types) =>
    [
        .. types.Where(t => t.IsValueType && typeof(IStronglyTypedId).IsAssignableFrom(t) && !t.Name.EndsWith("Id", StringComparison.Ordinal) && !t.Name.EndsWith("Ref", StringComparison.Ordinal))
            .Select(t => $"{t.FullName}：强类型 ID 的名字要以 Id 或 Ref 结尾。"),
    ];

    /// <summary>
    /// 声明 Error 的静态类叫 *Errors，错误码以模块名开头（框架是 Common）
    /// </summary>
    public static IReadOnlyList<string> Errors(IEnumerable<Type> types, string module)
    {
        var problems = new List<string>();
        var prefix = string.Equals(module, "BuildingBlocks", StringComparison.Ordinal) ? "Common." : module + ".";
        foreach (var type in types.Where(t => t is { IsAbstract: true, IsSealed: true }))
        {
            var errors = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Error))
                .Select(f => (Error)f.GetValue(null)!)
                .ToList();
            if (errors.Count == 0)
            {
                continue;
            }

            if (!type.Name.EndsWith("Errors", StringComparison.Ordinal))
            {
                problems.Add($"{type.FullName}：声明错误的静态类要以 Errors 结尾。");
            }

            problems.AddRange(errors.Where(e => !e.Code.StartsWith(prefix, StringComparison.Ordinal)).Select(e => $"{type.FullName}：错误码 {e.Code} 要以 {prefix} 开头。"));
        }

        return problems;
    }

    private static string? MessageSuffix(Type type)
    {
        if (IsCommand(type))
        {
            return "Command";
        }

        return IsQuery(type) ? "Query" : null;
    }

    private static string? PartKind(Type type)
    {
        if (IsValidator(type))
        {
            return "Validator";
        }

        return typeof(IEndpoint).IsAssignableFrom(type) ? "Endpoint" : null;
    }

    private static bool IsConcrete(Type type) =>
        type is { IsClass: true, IsAbstract: false } && type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() is null;

    private static bool IsCommand(Type type) =>
        typeof(ICommand).IsAssignableFrom(type) || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

    private static bool IsQuery(Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

    private static bool IsHandler(Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() is var d
            && (d == typeof(ICommandHandler<>) || d == typeof(ICommandHandler<,>) || d == typeof(IQueryHandler<,>)));

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

    // 编译器给 record 生成 <Clone>$ 方法
    private static bool IsRecord(Type type) => type.GetMethod("<Clone>$") is not null;
}
