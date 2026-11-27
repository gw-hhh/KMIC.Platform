using System.Reflection;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.ArchitectureTests.Rules;

internal static class NamingRules
{
    // 命令以 Command 结尾，查询以 Query 结尾，且均为 sealed record
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

    // 处理器须为 public sealed 且以 Handler 结尾（Mediator 生成的分发代码需直接引用）
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

    // 校验器与端点须为 internal sealed，分别以 Validator、Endpoint 结尾
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

    // 领域事件须为 sealed record
    public static IReadOnlyList<string> DomainEvents(IEnumerable<Type> types) =>
    [
        .. types.Where(t => IsConcrete(t) && typeof(IDomainEvent).IsAssignableFrom(t) && (!t.IsSealed || !IsRecord(t)))
            .Select(t => $"{t.FullName}：领域事件要写成 sealed record。"),
    ];

    // 强类型 ID 须以 Id 或 Ref 结尾
    public static IReadOnlyList<string> Identifiers(IEnumerable<Type> types) =>
    [
        .. types.Where(t => t.IsValueType && typeof(IStronglyTypedId).IsAssignableFrom(t) && !t.Name.EndsWith("Id", StringComparison.Ordinal) && !t.Name.EndsWith("Ref", StringComparison.Ordinal))
            .Select(t => $"{t.FullName}：强类型 ID 的名字要以 Id 或 Ref 结尾。"),
    ];

    // 声明 Error 的静态类须命名为 *Errors，错误码以模块名（框架为 Common）开头
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
