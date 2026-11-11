using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一个模块声明的订阅，校验放在订阅目录里统一做
/// </summary>
internal sealed class ModuleSubscriptions(string module) : IMessagingBuilder
{
    private readonly List<DeclaredSubscription> _subscriptions = [];
    private readonly List<RetiredSubscription> _retirements = [];

    public string Module { get; } = module;

    public IReadOnlyList<DeclaredSubscription> Subscriptions => _subscriptions;

    public IReadOnlyList<RetiredSubscription> Retirements => _retirements;

    public void Subscribe<TMessage, TCommand>(string name, Func<TMessage, MessageEnvelope, TCommand> map, Action<SubscriptionOptions>? configure = null)
        where TMessage : class, IIntegrationMessage
        where TCommand : ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(map);

        var options = new SubscriptionOptions();
        configure?.Invoke(options);
        _subscriptions.Add(new DeclaredSubscription(name, typeof(TMessage), typeof(TCommand), (message, envelope) => map((TMessage)message, envelope), options));
    }

    public void Retired(string name, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        _retirements.Add(new RetiredSubscription(name, Module, reason));
    }
}
