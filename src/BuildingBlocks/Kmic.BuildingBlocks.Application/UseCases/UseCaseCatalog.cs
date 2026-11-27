using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;

namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>用例目录，声明缺失或冲突时拒绝启动。</summary>
public sealed class UseCaseCatalog
{
    private readonly FrozenDictionary<Type, UseCase> _byType;

    private UseCaseCatalog(IEnumerable<UseCase> useCases) => _byType = useCases.ToFrozenDictionary(u => u.MessageType);

    public IReadOnlyCollection<UseCase> All => _byType.Values;

    public static UseCaseCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => IsMessage(t) || t.IsDefined(typeof(BypassesDataScopeAttribute), inherit: false)));
    }

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

    public UseCase Get(Type messageType) =>
        _byType.TryGetValue(messageType, out var useCase)
            ? useCase
            : throw new InvalidOperationException($"{messageType.FullName} 不在用例目录中，请检查宿主是否已注册其所在模块。");

    /// <summary>查找消息对应的用例定义，不存在时返回 null。</summary>
    public UseCase? Find(Type messageType) => _byType.GetValueOrDefault(messageType);

    // 同时实现命令与查询接口的类型也需纳入扫描，以便统一报告冲突
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
            problems.Add($"{name}：不能同时实现命令与查询接口。");
            return null;
        }

        var countBefore = problems.Count;
        var access = ReadAccess(type, name, kind, problems);
        var audit = ReadAudit(type, name, kind, problems);
        var featureGate = ReadFeatureGate(type, name, problems);
        var exempt = Has<ExemptFromMessagingSuspensionAttribute>(type);
        if (exempt && kind != MessageKind.Command)
        {
            problems.Add($"{name}：[ExemptFromMessagingSuspension] 只能用于命令。");
        }

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
            AllowWhenImpersonationReadOnly = access.AllowWhenImpersonationReadOnly,
            ExemptFromMessagingSuspension = exempt,
            ContractAccess = access.ContractAccess,
            Idempotent = audit.Idempotent,
            Audited = kind == MessageKind.Command ? audit.Audited is not null : null,
            AuditAction = AuditActionOf(audit, module, type),
            AuditedRead = audit.AuditedRead,
            FeatureGate = featureGate,
            BypassesDataScope = bypassesDataScope,
        };
    }

    // 开关名格式为“模块.功能”，命名规则与模块编码一致
    private static string? ReadFeatureGate(Type type, string name, List<string> problems)
    {
        if (type.GetCustomAttribute<FeatureGateAttribute>() is not { } gate)
        {
            return null;
        }

        if (!ModuleCodes.IsValidFeatureName(gate.Name))
        {
            problems.Add($"{name}：功能开关名 \"{gate.Name}\" 格式无效，须为“模块.功能”，如 Sample.NewImport。");
            return null;
        }

        return gate.Name;
    }

    // [BypassesDataScope] 标注在处理器上，需关联记录到其处理的消息名下
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

    // 授权声明须有且仅有一种；AllowSystem、AllowWhenPasswordChangeRequired、AllowWhenImpersonationReadOnly 仅可与特定声明组合
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
        var allowWhenImpersonationReadOnly = Has<AllowWhenImpersonationReadOnlyAttribute>(type);
        if (allowSystem && requirement is not (AccessRequirement.Permission or AccessRequirement.Authenticated))
        {
            problems.Add($"{name}：[AllowSystem] 只能与 [RequirePermission] 或 [AllowAuthenticated] 叠加。");
        }

        if (allowWhenPasswordChangeRequired && requirement != AccessRequirement.Authenticated)
        {
            problems.Add($"{name}：[AllowWhenPasswordChangeRequired] 只能与 [AllowAuthenticated] 叠加。");
        }

        if (allowWhenImpersonationReadOnly && (requirement != AccessRequirement.Authenticated || kind != MessageKind.Command))
        {
            problems.Add($"{name}：[AllowWhenImpersonationReadOnly] 只能用于带 [AllowAuthenticated] 的命令。");
        }

        if (permission is not null && string.IsNullOrWhiteSpace(permission.Permission))
        {
            problems.Add($"{name}：[RequirePermission] 的权限点编码不能为空。");
        }

        if (contractQuery is not null && kind != MessageKind.Query)
        {
            problems.Add($"{name}：[ContractQuery] 只能用于查询。");
        }

        if (contractQuery is not null && kind == MessageKind.Query)
        {
            ReferenceResultRule.Check(type, name, contractQuery, problems);
        }

        return new AccessDeclaration(requirement, permission?.Permission, allowSystem, allowWhenPasswordChangeRequired, allowWhenImpersonationReadOnly, contractQuery?.Access);
    }

    // 幂等与审计仅限命令，读取审计仅限查询，所有命令均须显式声明是否审计
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

            // 模块间契约查询仅用于向业务下载查询提供文件元信息，读取审计由业务下载查询记录
            if (!auditedRead && !Has<ContractQueryAttribute>(type) && ReturnsFileDownload(type))
            {
                problems.Add($"{name}：返回 FileDownload 的查询必须 [AuditedRead]，每次下载都要留审计记录。");
            }

            return new AuditDeclaration(false, null, auditedRead);
        }

        if ((audited is not null) == notAudited)
        {
            problems.Add($"{name}：命令必须且只能有 [Audited]、[NotAudited] 之一。");
        }

        if (idempotent && audited is null)
        {
            problems.Add($"{name}：标注 [Idempotent] 的命令必须 [Audited]。");
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

    // 命令取 [Audited].Action 或“模块.命令名”，敏感查询取“模块.查询名”
    private static string? AuditActionOf(AuditDeclaration audit, string module, Type type)
    {
        if (audit.Audited is { } audited)
        {
            return audited.Action ?? $"{module}.{TrimSuffix(type.Name, "Command")}";
        }

        return audit.AuditedRead ? $"{module}.{TrimSuffix(type.Name, "Query")}" : null;
    }
    private static bool ReturnsFileDownload(Type queryType) =>
        queryType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>) && i.GetGenericArguments()[0] == typeof(FileDownload));

    private static string TrimSuffix(string name, string suffix) =>
        name.EndsWith(suffix, StringComparison.Ordinal) ? name[..^suffix.Length] : name;

    private static bool Has<TAttribute>(Type type)
        where TAttribute : Attribute => type.IsDefined(typeof(TAttribute), inherit: false);

    private sealed record AccessDeclaration(
        AccessRequirement Requirement,
        string? Permission,
        bool AllowSystem,
        bool AllowWhenPasswordChangeRequired,
        bool AllowWhenImpersonationReadOnly,
        ContractAccess? ContractAccess);

    private sealed record AuditDeclaration(bool Idempotent, AuditedAttribute? Audited, bool AuditedRead);
}
