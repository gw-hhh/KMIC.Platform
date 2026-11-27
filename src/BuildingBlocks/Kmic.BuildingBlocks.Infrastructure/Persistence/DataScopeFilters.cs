using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

internal static class DataScopeFilters
{
    private static readonly MethodInfo _containsOrgUnit = typeof(Enumerable).GetMethods()
        .Single(m => string.Equals(m.Name, nameof(Enumerable.Contains), StringComparison.Ordinal) && m.GetParameters().Length == 2)
        .MakeGenericMethod(typeof(OrgUnitId));

    public static void Apply(ModelBuilder modelBuilder, ModuleDbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            if (entityType.FindAnnotation(ModelAnnotations.DataScope)?.Value is string scopeObject)
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(QueryFilters.DataScope, Build(entityType.ClrType, scopeObject, context));
            }
        }
    }

    private static LambdaExpression Build(Type clrType, string scopeObject, ModuleDbContext context)
    {
        var entity = Expression.Parameter(clrType, "e");

        // EF Core 会将表达式中的上下文参数替换为实际执行查询的上下文实例，从而始终读取当前请求的数据范围
        var self = Expression.Constant(context);
        var scope = Expression.Call(self, nameof(ModuleDbContext.DataScopeFor), Type.EmptyTypes, Expression.Constant(scopeObject));

        var granted = new List<Expression> { Expression.Property(scope, nameof(EffectiveScope.All)) };
        if (typeof(IHasOwner).IsAssignableFrom(clrType))
        {
            var owner = Expression.Convert(Expression.Property(entity, nameof(IHasOwner.OwnerId)), typeof(UserId?));
            granted.Add(Expression.Equal(owner, Expression.Property(scope, nameof(EffectiveScope.Owner))));
        }

        if (typeof(IHasOrgUnit).IsAssignableFrom(clrType))
        {
            var orgUnits = Expression.Property(scope, nameof(EffectiveScope.OrgUnits));
            granted.Add(Expression.Call(_containsOrgUnit, orgUnits, Expression.Property(entity, nameof(IHasOrgUnit.OrgUnitId))));
        }

        var unrestricted = Expression.Property(self, nameof(ModuleDbContext.DataScopeUnrestricted));
        return Expression.Lambda(Expression.OrElse(unrestricted, granted.Aggregate(Expression.OrElse)), entity);
    }
}
