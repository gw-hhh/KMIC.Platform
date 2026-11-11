using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal static class EntityKinds
{
    // 框架表和读模型不继承 Entity<TId>，审计列、版本号这些约定只加在继承它的业务实体上
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
