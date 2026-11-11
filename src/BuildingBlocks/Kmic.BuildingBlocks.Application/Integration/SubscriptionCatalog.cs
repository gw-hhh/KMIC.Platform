using System.Collections.Frozen;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 订阅目录
/// </summary>
public sealed class SubscriptionCatalog
{
    private readonly FrozenDictionary<(string Name, int Version), SubscriptionDefinition> _byVersion;

    private SubscriptionCatalog(IReadOnlyList<SubscriptionDefinition> subscriptions, IReadOnlyList<RetiredSubscription> retired)
    {
        All = subscriptions;
        Retired = retired;
        _byVersion = subscriptions.ToFrozenDictionary(s => (s.Name, s.Message.Version));
    }

    internal IReadOnlyList<SubscriptionDefinition> All { get; }

    internal IReadOnlyList<RetiredSubscription> Retired { get; }

    internal static SubscriptionCatalog Build(IEnumerable<ModuleSubscriptions> modules, UseCaseCatalog useCases, IntegrationMessageCatalog messages)
    {
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(useCases);
        ArgumentNullException.ThrowIfNull(messages);

        var problems = new List<string>();
        var declared = modules.ToList();
        var subscriptions = declared
            .SelectMany(m => m.Subscriptions.Select(s => Read(m.Module, s, useCases, messages, problems)))
            .OfType<SubscriptionDefinition>()
            .ToList();
        var retired = declared.SelectMany(m => m.Retirements).ToList();

        CheckNames(subscriptions, problems);
        CheckRequests(subscriptions, messages, problems);
        CheckRetired(retired, subscriptions, problems);

        return problems.Count > 0 ? throw new DeclarationException("订阅声明", problems) : new SubscriptionCatalog(subscriptions, retired);
    }

    internal SubscriptionDefinition? Find(string subscription, int version) => _byVersion.GetValueOrDefault((subscription, version));

    private static SubscriptionDefinition? Read(string module, DeclaredSubscription declared, UseCaseCatalog useCases, IntegrationMessageCatalog messages, List<string> problems)
    {
        var name = declared.Name;
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"订阅名“{name}”要以“{module}.”开头。");
        }

        if (messages.Find(declared.MessageType) is not { } message)
        {
            problems.Add($"订阅 {name} 订阅的 {declared.MessageType.Name} 不在集成消息目录中。");
            return null;
        }

        var useCase = useCases.Find(declared.CommandType);
        if (useCase is null || useCase.Kind != MessageKind.Command || !string.Equals(useCase.Module, module, StringComparison.Ordinal))
        {
            problems.Add($"订阅 {name} 映射的 {declared.CommandType.Name} 要是 {module} 模块的命令。");
        }
        else if (!useCase.AllowSystem && useCase.Requirement != AccessRequirement.SystemOnly)
        {
            problems.Add($"订阅 {name} 映射的 {declared.CommandType.Name} 以系统身份执行，要标 [SystemOnly] 或 [AllowSystem]。");
        }

        if (message.IsRequest && !string.Equals(message.Module, module, StringComparison.Ordinal))
        {
            problems.Add($"{message.Name} 是请求型消息，只能由定义它的 {message.Module} 模块接收。");
        }
        else if (!message.IsRequest && string.Equals(message.Module, module, StringComparison.Ordinal))
        {
            problems.Add($"订阅 {name} 订阅了本模块自己发布的 {message.Name}，模块内部要用领域事件。");
        }

        if (declared.Options.Rebuild is { } rebuild && !string.Equals(ModuleNames.Of(rebuild.ReadModel), module, StringComparison.Ordinal))
        {
            problems.Add($"订阅 {name} 重建的读模型 {rebuild.ReadModel.Name} 要在 {module} 模块里。");
        }

        return new SubscriptionDefinition(name, module, message, declared.CommandType, declared.Map, declared.Options.IsOrdered, declared.Options.Rebuild);
    }

    // 同一个订阅可以分别处理同一种消息的几个版本，但不能混订不同的消息
    private static void CheckNames(List<SubscriptionDefinition> subscriptions, List<string> problems)
    {
        foreach (var group in subscriptions.GroupBy(s => s.Name, StringComparer.Ordinal))
        {
            if (group.Select(s => s.Message.Name).Distinct(StringComparer.Ordinal).Count() > 1)
            {
                problems.Add($"订阅名“{group.Key}”用在了不同的消息上：{string.Join("、", group.Select(s => s.Message.Name).Distinct(StringComparer.Ordinal))}。");
            }
            else if (group.GroupBy(s => s.Message.Version).Any(v => v.Count() > 1))
            {
                problems.Add($"订阅“{group.Key}”对同一个消息版本声明了两次。");
            }

            // 重建按订阅名排队，几个消息版本都写了重建的话，读模型和来源要是同一个
            var rebuilds = group.Select(s => s.Rebuild).OfType<IReadModelRebuild>().ToList();
            if (rebuilds.Select(r => (r.ReadModel, r.Source)).Distinct().Count() > 1)
            {
                problems.Add($"订阅“{group.Key}”的几个消息版本声明了不同的重建方式，要写成同一个读模型和同一个来源。");
            }
        }
    }

    private static void CheckRequests(List<SubscriptionDefinition> subscriptions, IntegrationMessageCatalog messages, List<string> problems)
    {
        foreach (var request in messages.All.Where(m => m.IsRequest))
        {
            var count = subscriptions.Count(s => s.Message == request);
            if (count != 1)
            {
                problems.Add($"请求型消息 {request.Name} 版本 {request.Version} 要有且只有一个订阅，现在有 {count} 个。");
            }
        }
    }

    private static void CheckRetired(List<RetiredSubscription> retired, List<SubscriptionDefinition> subscriptions, List<string> problems)
    {
        foreach (var item in retired)
        {
            if (!item.Name.StartsWith(item.Module + ".", StringComparison.Ordinal))
            {
                problems.Add($"退役的订阅“{item.Name}”要以“{item.Module}.”开头。");
            }

            if (subscriptions.Any(s => string.Equals(s.Name, item.Name, StringComparison.Ordinal)))
            {
                problems.Add($"订阅“{item.Name}”声明了退役，就不能同时再订阅。");
            }
        }

        foreach (var duplicate in retired.GroupBy(r => r.Name, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"订阅“{duplicate.Key}”的退役声明写了两次。");
        }
    }
}
