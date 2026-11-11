using System.Collections.Concurrent;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 按事件的运行时类型找到全部处理器并依次执行，任何一个失败即停止，整个命令随之失败
/// </summary>
/// <remarks>
/// 领域事件不经 Mediator（05-01 第 8 节）；同一事件的多个处理器不保证顺序
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
