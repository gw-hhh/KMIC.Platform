using System.Collections.Concurrent;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 领域事件分发
/// </summary>
/// <remarks>
/// 依次执行事件的全部处理器，有一个失败就停止。领域事件不经过 Mediator，多个处理器之间不保证顺序
/// </remarks>
internal sealed class DomainEventDispatcher(IServiceProvider services)
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, IDomainEvent, CancellationToken, ValueTask<Result>>> _dispatchers = new();

    private static readonly MethodInfo _dispatchTyped =
        typeof(DomainEventDispatcher).GetMethod(nameof(DispatchTypedAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public ValueTask<Result> DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        return _dispatchers.GetOrAdd(domainEvent.GetType(), Build)(services, domainEvent, ct);
    }

    private static Func<IServiceProvider, IDomainEvent, CancellationToken, ValueTask<Result>> Build(Type eventType) =>
        _dispatchTyped.MakeGenericMethod(eventType).CreateDelegate<Func<IServiceProvider, IDomainEvent, CancellationToken, ValueTask<Result>>>();

    private static async ValueTask<Result> DispatchTypedAsync<TEvent>(IServiceProvider services, IDomainEvent domainEvent, CancellationToken ct)
        where TEvent : IDomainEvent
    {
        foreach (var handler in services.GetServices<IDomainEventHandler<TEvent>>())
        {
            var result = await handler.HandleAsync((TEvent)domainEvent, ct);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }
}
