using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Events;

internal static class DomainEventHandlerRegistration
{
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
