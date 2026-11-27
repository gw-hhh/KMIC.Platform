using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

public sealed class IntegrationMessageCatalog
{
    private static readonly Assembly _frameworkContracts = typeof(IIntegrationMessage).Assembly;

    private readonly FrozenDictionary<Type, IntegrationMessageType> _byType;
    private readonly FrozenDictionary<(string Name, int Version), IntegrationMessageType> _byName;

    private IntegrationMessageCatalog(IReadOnlyCollection<IntegrationMessageType> types)
    {
        _byType = types.ToFrozenDictionary(t => t.ClrType);
        _byName = types.ToFrozenDictionary(t => (t.Name, t.Version));
    }

    public static IntegrationMessageCatalog Empty { get; } = new([]);

    /// <summary>从各模块的 Application 程序集及其引用的 Contracts 程序集中读取集成消息定义。</summary>
    public static IntegrationMessageCatalog FromAssemblies(IEnumerable<Assembly> applicationAssemblies)
    {
        ArgumentNullException.ThrowIfNull(applicationAssemblies);

        var assemblies = applicationAssemblies
            .SelectMany(a => a.GetReferencedAssemblies().Where(IsContracts).Select(Assembly.Load).Prepend(a))
            .Append(_frameworkContracts)
            .Distinct();
        return Build(assemblies.SelectMany(a => a.GetTypes()));
    }

    internal static IntegrationMessageCatalog Build(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var problems = new List<string>();
        var messages = types.Distinct()
            .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false } && t.IsAssignableTo(typeof(IIntegrationMessage)))
            .Select(t => Read(t, problems))
            .OfType<IntegrationMessageType>()
            .ToList();

        foreach (var duplicate in messages.GroupBy(m => (m.Name, m.Version)).Where(g => g.Count() > 1))
        {
            problems.Add($"消息名称“{duplicate.Key.Name}”版本 {duplicate.Key.Version} 重复：{string.Join("、", duplicate.Select(m => m.ClrType.FullName))}。");
        }

        return problems.Count > 0 ? throw new DeclarationException("集成消息声明", problems) : new IntegrationMessageCatalog(messages);
    }

    /// <summary>判断管理视图是否允许展示指定消息载荷；敏感消息及未识别类型均不显示。</summary>
    public bool CanShowPayload(string name, int version) => Find(name, version) is { CarriesSecret: false };

    internal IntegrationMessageType Describe(Type messageType) =>
        _byType.GetValueOrDefault(messageType)
            ?? throw new InvalidOperationException($"{messageType.FullName} 未收录于集成消息目录，请检查是否已标注 [IntegrationMessage] 且所属模块已注册。");

    internal IntegrationMessageType? Find(string name, int version) => _byName.GetValueOrDefault((name, version));

    internal IntegrationMessageType? Find(Type messageType) => _byType.GetValueOrDefault(messageType);

    internal IEnumerable<IntegrationMessageType> All => _byType.Values;

    private static bool IsContracts(AssemblyName name) =>
        name.Name is { } n && n.StartsWith("Kmic.", StringComparison.Ordinal) && n.EndsWith(".Contracts", StringComparison.Ordinal);

    private static IntegrationMessageType? Read(Type type, List<string> problems)
    {
        if (type.GetCustomAttribute<IntegrationMessageAttribute>() is not { } attribute)
        {
            problems.Add($"{type.FullName} 实现了集成消息接口，但未标注 [IntegrationMessage]。");
            return null;
        }

        var name = attribute.Name;
        var isEvent = type.IsAssignableTo(typeof(IIntegrationEvent));
        var isRequest = type.IsAssignableTo(typeof(IIntegrationRequest));
        if (isEvent == isRequest)
        {
            problems.Add($"{name} 必须且只能实现 IIntegrationEvent（事件）或 IIntegrationRequest（请求型消息）之一。");
            return null;
        }

        var module = type.Assembly == _frameworkContracts ? "Framework" : ModuleNames.Of(type);
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"{type.Name} 的消息名称“{name}”须以“{module}.”开头。");
        }

        if (attribute.Version < 1)
        {
            problems.Add($"{name} 的版本号须从 1 开始。");
        }

        if (isRequest && !PlatformModules.Names.Contains(module))
        {
            problems.Add($"{name} 是请求型消息，只能定义在平台模块（{string.Join("、", PlatformModules.Names)}）里，业务模块之间用事件。");
        }

        return new IntegrationMessageType(
            name,
            attribute.Version,
            isRequest,
            type,
            module,
            ReadSubjects(type, name, isRequest, problems),
            ReadOrderingKey(type, name, isRequest, problems),
            ReadSecretLifetime(attribute, name, isRequest, problems));
    }

    private static int? ReadSecretLifetime(IntegrationMessageAttribute attribute, string name, bool isRequest, List<string> problems)
    {
        if (!attribute.CarriesSecret)
        {
            if (attribute.SecretLifetimeSeconds != 0)
            {
                problems.Add($"{name} 已设置凭据有效期，但没有声明 CarriesSecret。");
            }

            return null;
        }

        if (!isRequest)
        {
            problems.Add($"{name} 是事件，不能声明 CarriesSecret，只有请求型消息可以。");
            return null;
        }

        if (attribute.SecretLifetimeSeconds is < 1 or > IntegrationMessageAttribute.MaxSecretLifetimeSeconds)
        {
            problems.Add($"{name} 声明了 CarriesSecret，凭据有效期 SecretLifetimeSeconds 须位于 1 到 {IntegrationMessageAttribute.MaxSecretLifetimeSeconds} 秒之间。");
            return null;
        }

        return attribute.SecretLifetimeSeconds;
    }

    private static PropertyInfo? ReadSubjects(Type type, string name, bool isRequest, List<string> problems)
    {
        if (type.GetCustomAttribute<MessageSubjectsAttribute>() is not { } subjects)
        {
            return null;
        }

        if (!isRequest)
        {
            problems.Add($"{name} 是事件，不可标注 [MessageSubjects]，只有请求型消息可以。");
            return null;
        }

        var property = type.GetProperty(subjects.PropertyName);
        if (property is null || (property.PropertyType != typeof(Guid) && !property.PropertyType.IsAssignableTo(typeof(IEnumerable<Guid>))))
        {
            problems.Add($"{name} 的 [MessageSubjects] 指向的属性 {subjects.PropertyName} 不存在，或者不是 Guid / Guid 的集合。");
            return null;
        }

        return property;
    }

    private static PropertyInfo? ReadOrderingKey(Type type, string name, bool isRequest, List<string> problems)
    {
        if (type.GetCustomAttribute<OrderingKeyAttribute>() is not { } orderingKey)
        {
            return null;
        }

        if (isRequest)
        {
            problems.Add($"{name} 是请求型消息，不可标注 [OrderingKey]，只有事件可以。");
            return null;
        }

        var property = type.GetProperty(orderingKey.PropertyName);
        if (property is null || (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(string)))
        {
            problems.Add($"{name} 的 [OrderingKey] 指向的属性 {orderingKey.PropertyName} 不存在，或者不是 Guid / string。");
            return null;
        }

        return property;
    }
}
