using System.Reflection;
using System.Runtime.CompilerServices;
using FluentValidation;
using Kmic.ArchitectureTests.Support;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.ArchitectureTests.Rules;

/// <summary>
/// 用例与注册的结构规则
/// </summary>
internal static class StructureRules
{
    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static readonly string[] _sensitiveNames = ["Purge", "Impersonate", "Export"];

    private static readonly HashSet<Type> _mutableCollections = [typeof(List<>), typeof(Dictionary<,>), typeof(HashSet<>), typeof(IList<>), typeof(ICollection<>), typeof(IDictionary<,>), typeof(ISet<>)];

    private static readonly string[] _idempotentVerbs = ["Create", "Submit", "Approve", "Pay"];

    /// <summary>
    /// 类型所在的模块名；框架、BCL、第三方库的类型返回 null
    /// </summary>
    public static string? ModuleOf(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var parts = type.Assembly.GetName().Name!.Split('.');
        return parts is ["Kmic", var module, _] && !string.Equals(module, "BuildingBlocks", StringComparison.Ordinal) ? module : null;
    }

    /// <summary>
    /// 命令的响应只是收据：属性只有标识、版本号、编码、计数、时刻、布尔值，没有嵌套对象；集合只允许“逐条收据”，元素同样不再嵌套
    /// </summary>
    /// <remarks>
    /// 语义上收据里是不是只有本人可取回的最小信息，由评审判断；这里只检查结构。系统专用命令不受限
    /// </remarks>
    public static IReadOnlyList<string> CommandResponses(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var command in types.Where(t => t is { IsClass: true, IsAbstract: false } && t.GetCustomAttribute<SystemOnlyAttribute>() is null))
        {
            var response = command.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault();
            if (response is null)
            {
                continue;
            }

            foreach (var property in response.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (ElementOf(propertyType) is { } element)
                {
                    problems.AddRange(ReceiptItemProblems(command, response, property, element));
                }
                else if (!IsReceiptValue(propertyType))
                {
                    problems.Add($"{command.FullName}：响应 {response.Name} 的属性 {property.Name}（{propertyType.Name}）不是收据允许的类型，命令只返回标识、版本号、编码、计数、时刻、布尔值，业务内容经查询获取。");
                }
            }
        }

        return problems;
    }

    /// <summary>
    /// 命令、查询处理器只由 Mediator 生成的代码引用：产品代码里没有别的类型把它们当参数、字段、属性或返回值
    /// </summary>
    /// <remarks>
    /// 只检查成员的签名，不读方法体；Mediator 生成的代码在宿主程序集里，不在检查范围内
    /// </remarks>
    public static IReadOnlyList<string> HandlerReferences(IEnumerable<Type> types)
    {
        var all = types.Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null).ToList();
        var handlers = all.Where(IsMessageHandler).ToHashSet();
        var problems = new List<string>();
        foreach (var type in all.Where(t => !handlers.Contains(t)))
        {
            foreach (var referenced in SignatureTypes(type).Where(handlers.Contains).Distinct())
            {
                problems.Add($"{type.FullName}：不能引用处理器 {referenced.Name}，处理器只由发送器调用，要复用的逻辑放进领域服务。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 领域事件处理器和命令在同一个事务里执行，只能依赖本模块和框架，不注入别的模块的任何类型
    /// </summary>
    /// <param name="types">要检查的类型</param>
    /// <param name="moduleOf">类型所在的模块，框架类型返回 null</param>
    public static IReadOnlyList<string> DomainEventHandlers(IEnumerable<Type> types, Func<Type, string?> moduleOf)
    {
        ArgumentNullException.ThrowIfNull(moduleOf);

        var problems = new List<string>();
        foreach (var handler in types.Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))))
        {
            var own = moduleOf(handler);
            foreach (var injected in handler.GetConstructors(AllMembers).SelectMany(c => c.GetParameters()).Select(p => p.ParameterType))
            {
                if (moduleOf(injected) is { } other && !string.Equals(other, own, StringComparison.Ordinal))
                {
                    problems.Add($"{handler.FullName}：领域事件处理器不能依赖 {other} 模块的 {injected.Name}，它和命令在同一个事务里，只能改本模块的数据。");
                }
            }
        }

        return problems;
    }

    /// <summary>
    /// 服务注册只在模块的注册类里：别的类型不接收也不返回服务集合
    /// </summary>
    /// <remarks>
    /// 模块里的扩展方法类也算违规：它们会让注册散落在注册类之外，读代码的人找不到服务是在哪里登记的
    /// </remarks>
    public static IReadOnlyList<string> ServiceRegistration(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null && !typeof(IModule).IsAssignableFrom(t)))
        {
            var offending = type.GetMethods(AllMembers).Cast<MethodBase>().Concat(type.GetConstructors(AllMembers))
                .Where(m => m.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(IServiceCollection)) || (m is MethodInfo info && info.ReturnType == typeof(IServiceCollection)))
                .Select(m => m.Name)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            problems.AddRange(offending.Select(name => $"{type.FullName}.{name}：服务注册只写在模块的 IModule 实现里。"));
        }

        return problems;
    }

    /// <summary>
    /// 每个聚合根的类型初始化都成功：状态机在静态字段里构建，声明有误时第一次使用才抛异常，这里提前触发
    /// </summary>
    public static IReadOnlyList<string> AggregateInitialization(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var aggregate in types.Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false } && IsAggregateRoot(t)))
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(aggregate.TypeHandle);
            }
            catch (TypeInitializationException exception)
            {
                problems.Add($"{aggregate.FullName}：类型初始化失败（{exception.InnerException?.Message ?? exception.Message}）。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 名字表明是物理删除、模拟身份、导出全部数据的权限点，必须标 Sensitive
    /// </summary>
    /// <remarks>
    /// 按命名提示：权限点编码的最后一段以 Purge、Impersonate、Export 开头即视为敏感。标了 Sensitive 的权限点，用到它的命令必须审计
    /// </remarks>
    public static IReadOnlyList<string> SensitivePermissions(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var group in types.Where(t => t.GetCustomAttribute<PermissionGroupAttribute>() is not null))
        {
            foreach (var field in group.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral))
            {
                if (field.GetCustomAttribute<PermissionAttribute>() is not { Sensitive: false } || field.GetRawConstantValue() is not string code)
                {
                    continue;
                }

                var action = code[(code.LastIndexOf('.') + 1)..];
                if (_sensitiveNames.Any(prefix => action.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    problems.Add($"{group.FullName}.{field.Name}（{code}）：物理删除、模拟身份、导出全部数据的权限点必须标 Sensitive = true。");
                }
            }
        }

        return problems;
    }

    /// <summary>
    /// 读模型只能经 <c>IReadModelWriter&lt;T&gt;</c> 写入：读模型不是聚合根，也没有仓储
    /// </summary>
    public static IReadOnlyList<string> ReadModels(IEnumerable<Type> types)
    {
        var all = types.ToList();
        var readModels = all.Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IReadModel).IsAssignableFrom(t)).ToHashSet();
        var problems = readModels.Where(IsAggregateRoot)
            .Select(t => $"{t.FullName}：读模型不能是聚合根，只能经 IReadModelWriter 写入。")
            .ToList();
        foreach (var repository in all.Where(t => t is { IsClass: true, IsAbstract: false }))
        {
            problems.AddRange(repository.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<,>) && readModels.Contains(i.GetGenericArguments()[0]))
                .Select(i => $"{repository.FullName}：不能为读模型 {i.GetGenericArguments()[0].Name} 建仓储，只能经 IReadModelWriter 写入。"));
        }

        return problems;
    }

    /// <summary>
    /// 创建、提交、审批、支付类命令要么标 [Idempotent]，要么用 [NotIdempotent] 写明原因
    /// </summary>
    /// <remarks>
    /// 按命名提示：命令名以 Create、Submit、Approve、Pay 开头。系统专用命令不经 HTTP 调用，不受限
    /// </remarks>
    public static IReadOnlyList<string> IdempotencyDeclarations(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var command in types.Where(t => t is { IsClass: true, IsAbstract: false } && t.GetCustomAttribute<SystemOnlyAttribute>() is null && IsCommand(t)))
        {
            var verb = _idempotentVerbs.FirstOrDefault(v => command.Name.StartsWith(v, StringComparison.Ordinal));
            if (verb is not null && command.GetCustomAttribute<IdempotentAttribute>() is null && command.GetCustomAttribute<NotIdempotentAttribute>() is null)
            {
                problems.Add($"{command.FullName}：{verb} 类命令要标 [Idempotent]，确实不需要时用 [NotIdempotent(原因)] 写明。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 校验器不注入别的类型的 Contracts：校验器只看命令本身，要查其他模块的数据放进处理器
    /// </summary>
    /// <param name="types">要检查的类型</param>
    /// <param name="isContractsType">是否模块 Contracts 里的类型</param>
    public static IReadOnlyList<string> ValidatorsAndContracts(IEnumerable<Type> types, Func<Type, bool> isContractsType)
    {
        ArgumentNullException.ThrowIfNull(isContractsType);

        var problems = new List<string>();
        foreach (var validator in types.Where(IsValidator))
        {
            problems.AddRange(validator.GetConstructors(AllMembers).SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(isContractsType)
                .Select(p => $"{validator.FullName}：校验器不能注入 Contracts 里的 {p.Name}，跨模块的判断放在处理器里。"));
        }

        return problems;
    }

    /// <summary>
    /// 类型是否在某个模块的 Contracts 程序集里
    /// </summary>
    public static bool IsModuleContracts(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return ModuleOf(type) is not null && type.Assembly.GetName().Name!.EndsWith(".Contracts", StringComparison.Ordinal);
    }

    private static bool IsCommand(Type type) =>
        typeof(ICommand).IsAssignableFrom(type) || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

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

    /// <summary>
    /// 调用 <c>WithoutDataScope()</c> 的处理器必须标 [BypassesDataScope] 并写明原因，其他类型不能调用它
    /// </summary>
    /// <remarks>
    /// 读方法体的调用关系，含 async 方法和 lambda 生成的嵌套类型。绕过数据范围的用例会登记进用例目录，评审和审计能看到
    /// </remarks>
    public static IReadOnlyList<string> DataScopeBypasses(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null))
        {
            if (!IlCalls.CalledBy(type).Any(m => string.Equals(m.Name, nameof(QueryableExtensions.WithoutDataScope), StringComparison.Ordinal) && m.DeclaringType == typeof(QueryableExtensions)))
            {
                continue;
            }

            if (!IsMessageHandler(type))
            {
                problems.Add($"{type.FullName}：只有命令、查询处理器可以调用 WithoutDataScope，并且要标 [BypassesDataScope]。");
            }
            else if (type.GetCustomAttribute<BypassesDataScopeAttribute>() is null)
            {
                problems.Add($"{type.FullName}：调用了 WithoutDataScope，处理器要标 [BypassesDataScope] 并写明原因。");
            }
        }

        return problems;
    }

    /// <summary>
    /// 放进缓存的值不可变：缓存里的同一个对象会被多个请求共用，谁改了它，别人看到的就是被改过的数据
    /// </summary>
    /// <remarks>
    /// 读方法体，检查调用 HybridCache 时的泛型参数。可变指有公开的 set（init 不算）、公开的非只读字段、List、Dictionary、数组这类可修改的集合
    /// </remarks>
    public static IReadOnlyList<string> CacheValues(IEnumerable<Type> types)
    {
        var problems = new List<string>();
        foreach (var type in types.Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null))
        {
            var cached = IlCalls.CalledBy(type)
                .Where(m => m.DeclaringType == typeof(HybridCache) && m is MethodInfo { IsGenericMethod: true })
                .SelectMany(m => m.GetGenericArguments())
                .Distinct();
            foreach (var valueType in cached)
            {
                if (MutableMember(valueType, []) is { } mutable)
                {
                    problems.Add($"{type.FullName}：放进缓存的 {valueType.Name} 不是不可变的（{mutable}）。");
                }
            }
        }

        return problems;
    }

    // 返回第一个可变的成员说明，不可变时返回 null
    private static string? MutableMember(Type type, HashSet<Type> seen)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (IsImmutableLeaf(underlying) || !seen.Add(underlying))
        {
            return null;
        }

        if (underlying.IsArray)
        {
            return "数组可以被修改，用 IReadOnlyList";
        }

        if (underlying.IsGenericType)
        {
            var definition = underlying.GetGenericTypeDefinition();
            if (_mutableCollections.Contains(definition))
            {
                return $"{definition.Name} 可以被修改，用只读集合接口";
            }

            if (definition == typeof(IReadOnlyList<>) || definition == typeof(IReadOnlyCollection<>) || definition == typeof(IReadOnlyDictionary<,>) || definition == typeof(IEnumerable<>)
                || underlying.FullName?.StartsWith("System.ValueTuple", StringComparison.Ordinal) == true)
            {
                return underlying.GetGenericArguments().Select(a => MutableMember(a, seen)).FirstOrDefault(m => m is not null);
            }
        }

        foreach (var field in underlying.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => !f.IsInitOnly))
        {
            return $"公开字段 {field.Name} 可以被修改";
        }

        foreach (var property in underlying.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.SetMethod is { IsPublic: true } setter && !setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
            {
                return $"属性 {property.Name} 有公开的 set";
            }

            if (MutableMember(property.PropertyType, seen) is { } inner)
            {
                return $"属性 {property.Name}：{inner}";
            }
        }

        return null;
    }

    private static bool IsImmutableLeaf(Type type) =>
        type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(Guid) || type == typeof(decimal) || type == typeof(DateTimeOffset) || type == typeof(DateOnly) || type == typeof(TimeSpan)
        || typeof(IStronglyTypedId).IsAssignableFrom(type) || typeof(IEnumeration).IsAssignableFrom(type);

    private static bool IsAggregateRoot(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsMessageHandler(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && type.GetInterfaces().Any(i => i.IsGenericType
            && (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
                || i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
                || i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

    // 签名里出现的类型：构造函数与方法的参数和返回值、字段、属性，泛型参数展开
    private static IEnumerable<Type> SignatureTypes(Type type)
    {
        var members = type.GetMethods(AllMembers).Cast<MethodBase>().Concat(type.GetConstructors(AllMembers))
            .Where(m => m.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Concat(m is MethodInfo info ? [info.ReturnType] : []));
        var fields = type.GetFields(AllMembers).Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() is null).Select(f => f.FieldType);
        var properties = type.GetProperties(AllMembers).Select(p => p.PropertyType);
        return members.Concat(fields).Concat(properties).SelectMany(Unwrap);
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        yield return type;
        if (type.HasElementType)
        {
            foreach (var inner in Unwrap(type.GetElementType()!))
            {
                yield return inner;
            }
        }

        if (type.IsGenericType)
        {
            foreach (var inner in type.GetGenericArguments().SelectMany(Unwrap))
            {
                yield return inner;
            }
        }
    }

    // 集合属性的元素类型；不是集合（字符串除外）时返回 null
    private static Type? ElementOf(Type type)
    {
        if (type == typeof(string))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        var enumerable = type.GetInterfaces().Concat([type]).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumerable?.GetGenericArguments()[0];
    }

    private static IEnumerable<string> ReceiptItemProblems(Type command, Type response, PropertyInfo property, Type element)
    {
        if (IsReceiptValue(element))
        {
            yield break;
        }

        foreach (var inner in element.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var innerType = Nullable.GetUnderlyingType(inner.PropertyType) ?? inner.PropertyType;
            if (!IsReceiptValue(innerType))
            {
                yield return $"{command.FullName}：响应 {response.Name} 的集合 {property.Name} 只能是逐条收据，元素 {element.Name} 的属性 {inner.Name}（{innerType.Name}）不是收据允许的类型。";
            }
        }
    }

    private static bool IsReceiptValue(Type type) =>
        type.IsPrimitive
        || type.IsEnum
        || type == typeof(string)
        || type == typeof(Guid)
        || type == typeof(DateTimeOffset)
        || type == typeof(DateOnly)
        || typeof(IStronglyTypedId).IsAssignableFrom(type)
        || typeof(IEnumeration).IsAssignableFrom(type);
}
