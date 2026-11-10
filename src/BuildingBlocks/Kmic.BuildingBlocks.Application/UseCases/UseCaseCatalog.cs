using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 全部消息的横切声明，启动时构建一次并校验；行为在运行时只查这张表
/// </summary>
/// <remarks>
/// 声明缺失或冲突即拒绝启动，不存在“忘了写授权所以默认放行”（05-02 第 3 节）；同一套规则也作为架构测试进入检查
/// </remarks>
public sealed class UseCaseCatalog
{
    private readonly FrozenDictionary<Type, UseCase> _byType;

    private UseCaseCatalog(IEnumerable<UseCase> useCases) => _byType = useCases.ToFrozenDictionary(u => u.MessageType);

    /// <summary>
    /// 全部用例
    /// </summary>
    public IReadOnlyCollection<UseCase> All => _byType.Values;

    /// <summary>
    /// 扫描程序集中的全部命令、查询与处理器
    /// </summary>
    public static UseCaseCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => IsMessage(t) || t.IsDefined(typeof(BypassesDataScopeAttribute), inherit: false)));
    }

    /// <summary>
    /// 读取并校验给定类型中消息的声明，以及处理器上的 [BypassesDataScope]；有任何问题都汇总后一次报出
    /// </summary>
    public static UseCaseCatalog Build(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var distinct = types.Distinct().ToList();
        var problems = new List<string>();
        var bypasses = ReadBypasses(distinct.Where(t => !IsMessage(t)), problems);
        var useCases = new List<UseCase>();
        foreach (var type in distinct.Where(IsMessage))
        {
            var useCase = Read(type, bypasses.GetValueOrDefault(type), problems);
            if (useCase is not null)
            {
                useCases.Add(useCase);
            }
        }

        if (problems.Count > 0)
        {
            throw new DeclarationException("用例声明", problems);
        }

        return new UseCaseCatalog(useCases);
    }

    /// <summary>
    /// 取消息的用例；不在目录中说明宿主没有扫描到它所在的程序集，属于程序缺陷
    /// </summary>
    public UseCase Get(Type messageType) =>
        _byType.TryGetValue(messageType, out var useCase)
            ? useCase
            : throw new InvalidOperationException($"{messageType.FullName} 不在用例目录中，检查宿主是否注册了它所在的模块。");

    // 同时是命令和查询的类型也要进入 Read 报错，不能在扫描时漏掉；宿主中 Mediator 编译期已拒绝这种类型（MSG0004）
    private static bool IsMessage(Type type) =>
        type is { IsClass: true, IsAbstract: false } && (IsCommand(type) || IsQuery(type));

    private static MessageKind? KindOf(Type type) => (IsCommand(type), IsQuery(type)) switch
    {
        (true, false) => MessageKind.Command,
        (false, true) => MessageKind.Query,
        _ => null,
    };

    private static bool IsCommand(Type type) =>
        type.GetInterfaces().Any(i => i == typeof(ICommand) || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)));

    private static bool IsQuery(Type type) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

    private static UseCase? Read(Type type, string? bypassesDataScope, List<string> problems)
    {
        var name = type.FullName ?? type.Name;
        if (KindOf(type) is not { } kind)
        {
            problems.Add($"{name}：必须是命令或查询之一，不能两者都是。");
            return null;
        }

        var countBefore = problems.Count;
        var access = ReadAccess(type, name, kind, problems);
        var audit = ReadAudit(type, name, kind, problems);
        if (problems.Count > countBefore)
        {
            return null;
        }

        var module = ModuleNames.Of(type);
        return new UseCase
        {
            MessageType = type,
            Module = module,
            Kind = kind,
            Requirement = access.Requirement,
            Permission = access.Permission,
            AllowSystem = access.AllowSystem,
            AllowWhenPasswordChangeRequired = access.AllowWhenPasswordChangeRequired,
            ContractAccess = access.ContractAccess,
            Idempotent = audit.Idempotent,
            Audited = kind == MessageKind.Command ? audit.Audited is not null : null,
            AuditAction = audit.Audited is null ? null : audit.Audited.Action ?? $"{module}.{TrimSuffix(type.Name, "Command")}",
            AuditedRead = audit.AuditedRead,
            BypassesDataScope = bypassesDataScope,
        };
    }

    // [BypassesDataScope] 标在处理器上，记到它处理的消息名下
    private static Dictionary<Type, string> ReadBypasses(IEnumerable<Type> types, List<string> problems)
    {
        var bypasses = new Dictionary<Type, string>();
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<BypassesDataScopeAttribute>() is not { } attribute)
            {
                continue;
            }

            var name = type.FullName ?? type.Name;
            var messages = HandledMessages(type).ToList();
            if (messages.Count == 0)
            {
                problems.Add($"{name}：[BypassesDataScope] 只能标在命令或查询处理器上。");
            }

            if (string.IsNullOrWhiteSpace(attribute.Reason))
            {
                problems.Add($"{name}：[BypassesDataScope] 必须写明原因。");
            }

            foreach (var message in messages)
            {
                bypasses[message] = attribute.Reason;
            }
        }

        return bypasses;
    }

    private static IEnumerable<Type> HandledMessages(Type type) =>
        type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() is var definition
                && (definition == typeof(Mediator.ICommandHandler<,>) || definition == typeof(Mediator.IQueryHandler<,>)))
            .Select(i => i.GetGenericArguments()[0]);

    // 规则 1：授权声明必须且只能有一种；AllowSystem、AllowWhenPasswordChangeRequired 只能与特定声明叠加
    private static AccessDeclaration ReadAccess(Type type, string name, MessageKind kind, List<string> problems)
    {
        var permission = type.GetCustomAttribute<RequirePermissionAttribute>();
        var contractQuery = type.GetCustomAttribute<ContractQueryAttribute>();
        AccessRequirement[] declared =
        [
            .. new (bool Present, AccessRequirement Requirement)[]
            {
                (permission is not null, AccessRequirement.Permission),
                (Has<AllowAuthenticatedAttribute>(type), AccessRequirement.Authenticated),
                (Has<AllowAnonymousAttribute>(type), AccessRequirement.Anonymous),
                (Has<SystemOnlyAttribute>(type), AccessRequirement.SystemOnly),
                (contractQuery is not null, AccessRequirement.ContractQuery),
            }.Where(d => d.Present).Select(d => d.Requirement),
        ];

        if (declared.Length != 1)
        {
            problems.Add($"{name}：必须且只能有 [RequirePermission]、[AllowAuthenticated]、[AllowAnonymous]、[SystemOnly]、[ContractQuery] 之一，现在有 {declared.Length} 个。");
        }

        var requirement = declared.Length == 1 ? declared[0] : AccessRequirement.Permission;
        var allowSystem = Has<AllowSystemAttribute>(type);
        var allowWhenPasswordChangeRequired = Has<AllowWhenPasswordChangeRequiredAttribute>(type);
        if (allowSystem && requirement is not (AccessRequirement.Permission or AccessRequirement.Authenticated))
        {
            problems.Add($"{name}：[AllowSystem] 只能与 [RequirePermission] 或 [AllowAuthenticated] 叠加。");
        }

        if (allowWhenPasswordChangeRequired && requirement != AccessRequirement.Authenticated)
        {
            problems.Add($"{name}：[AllowWhenPasswordChangeRequired] 只能与 [AllowAuthenticated] 叠加。");
        }

        if (permission is not null && string.IsNullOrWhiteSpace(permission.Permission))
        {
            problems.Add($"{name}：[RequirePermission] 的权限点编码不能为空。");
        }

        if (contractQuery is not null && kind != MessageKind.Query)
        {
            problems.Add($"{name}：[ContractQuery] 只能用于查询。");
        }

        return new AccessDeclaration(requirement, permission?.Permission, allowSystem, allowWhenPasswordChangeRequired, contractQuery?.Access);
    }

    // 规则 2：幂等与审计声明只能用于命令（读取审计只能用于查询）；每个命令必须声明是否审计
    private static AuditDeclaration ReadAudit(Type type, string name, MessageKind kind, List<string> problems)
    {
        var audited = type.GetCustomAttribute<AuditedAttribute>();
        var notAudited = Has<NotAuditedAttribute>(type);
        var idempotent = Has<IdempotentAttribute>(type);
        var notIdempotent = Has<NotIdempotentAttribute>(type);
        var auditedRead = Has<AuditedReadAttribute>(type);

        if (kind == MessageKind.Query)
        {
            if (idempotent || audited is not null || notAudited || notIdempotent)
            {
                problems.Add($"{name}：[Idempotent]、[NotIdempotent]、[Audited]、[NotAudited] 只能用于命令。");
            }

            return new AuditDeclaration(false, null, auditedRead);
        }

        if ((audited is not null) == notAudited)
        {
            problems.Add($"{name}：命令必须且只能有 [Audited]、[NotAudited] 之一。");
        }

        if (idempotent && audited is null)
        {
            problems.Add($"{name}：标了 [Idempotent] 的命令必须 [Audited]。");
        }

        if (idempotent && notIdempotent)
        {
            problems.Add($"{name}：[Idempotent] 与 [NotIdempotent] 不能同时出现。");
        }

        if (auditedRead)
        {
            problems.Add($"{name}：[AuditedRead] 只能用于查询。");
        }

        return new AuditDeclaration(idempotent, audited, false);
    }

    private static string TrimSuffix(string name, string suffix) =>
        name.EndsWith(suffix, StringComparison.Ordinal) ? name[..^suffix.Length] : name;

    private static bool Has<TAttribute>(Type type)
        where TAttribute : Attribute => type.IsDefined(typeof(TAttribute), inherit: false);

    private sealed record AccessDeclaration(
        AccessRequirement Requirement,
        string? Permission,
        bool AllowSystem,
        bool AllowWhenPasswordChangeRequired,
        ContractAccess? ContractAccess);

    private sealed record AuditDeclaration(bool Idempotent, AuditedAttribute? Audited, bool AuditedRead);
}
