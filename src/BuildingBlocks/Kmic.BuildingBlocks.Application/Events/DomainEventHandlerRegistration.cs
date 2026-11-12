using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Events;

/// <summary>
/// 领域事件处理器的注册
/// </summary>
internal static class DomainEventHandlerRegistration
{
    /// <summary>
    /// 把程序集里实现了 <see cref="IDomainEventHandler{TEvent}" /> 的类注册为作用域服务
    /// </summary>
    /// <remarks>
    /// 一个事件可以有多个处理器，一个类也可以处理多个事件。处理器可以是 internal，不用模块的注册类逐个登记
    /// </remarks>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types.Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }).Distinct())
        {
            foreach (var handled in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            {
                services.AddScoped(handled, type);
            }
        }

        return services;
    }
}
