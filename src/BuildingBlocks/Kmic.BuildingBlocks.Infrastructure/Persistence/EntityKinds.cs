using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal static class EntityKinds
{
    // 业务实体都继承 Entity<TId>；框架表和读模型不继承，不加审计列这些约定
    public static bool IsBusinessEntity(Type? type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Entity<>))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAggregateRoot(Type? type) => type is not null && typeof(IAggregateRoot).IsAssignableFrom(type);
}
