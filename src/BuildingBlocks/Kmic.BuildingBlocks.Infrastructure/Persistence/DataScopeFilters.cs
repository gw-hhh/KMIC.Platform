using System.Linq.Expressions;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 数据权限过滤器
/// </summary>
/// <remarks>
/// 条件通过上下文成员读取当前请求的范围，EF 每次执行时把它们当参数传，查询只编译一次
/// </remarks>
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

        // EF 会把上下文实例换成执行查询的那个上下文，所以范围总是当前请求的
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
