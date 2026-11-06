using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Spike02.Domain;

namespace Spike02.Infrastructure;

// 构造：不受限 OR ( 资源可见性(e) AND ( 全部 OR e.OwnerId = 我 OR e.OrgUnitId = ANY(组织单元) OR 关系可见(e) ) )
internal static class DataScopeFilters
{
    private static readonly PropertyInfo _currentUserPlaceholder = typeof(DataScopeParam).GetProperty(nameof(DataScopeParam.CurrentUser))!;

    private static readonly MethodInfo _containsOrgUnit = typeof(Enumerable).GetMethods()
        .Single(m => string.Equals(m.Name, nameof(Enumerable.Contains), StringComparison.Ordinal) && m.GetParameters().Length == 2)
        .MakeGenericMethod(typeof(OrgUnitId));

    public static void Apply(ModelBuilder modelBuilder, ModuleDbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            if (entityType.FindAnnotation(DataScopeDefinition.AnnotationName)?.Value is not DataScopeDefinition definition)
            {
                continue;
            }

            entityType.RemoveAnnotation(DataScopeDefinition.AnnotationName);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ModuleDbContext.DataScopeFilter, Build(entityType.ClrType, definition, context));
        }
    }

    private static LambdaExpression Build(Type entityClrType, DataScopeDefinition definition, ModuleDbContext context)
    {
        var entity = Expression.Parameter(entityClrType, "e");
        var ctx = Expression.Constant(context);
        var currentUser = Expression.Property(ctx, nameof(ModuleDbContext.DataScopeCurrentUser));
        var scope = Expression.Call(ctx, nameof(ModuleDbContext.DataScopeFor), Type.EmptyTypes, Expression.Constant(definition.ScopeObject));

        var branches = new List<Expression> { Expression.Property(scope, nameof(EffectiveScope.All)) };
        if (typeof(IHasOwner).IsAssignableFrom(entityClrType))
        {
            var ownerId = Expression.Convert(Expression.Property(entity, nameof(IHasOwner.OwnerId)), typeof(UserId?));
            branches.Add(Expression.Equal(ownerId, Expression.Property(scope, nameof(EffectiveScope.Owner))));
        }

        if (typeof(IHasOrgUnit).IsAssignableFrom(entityClrType))
        {
            branches.Add(Expression.Call(
                _containsOrgUnit,
                Expression.Property(scope, nameof(EffectiveScope.OrgUnits)),
                Expression.Property(entity, nameof(IHasOrgUnit.OrgUnitId))));
        }

        if (definition.Relation is not null)
        {
            branches.Add(Inline(definition.Relation, entity, currentUser));
        }

        var granted = branches.Aggregate(Expression.OrElse);
        if (definition.Visibility is not null)
        {
            granted = Expression.AndAlso(Inline(definition.Visibility, entity, currentUser), granted);
        }

        var body = Expression.OrElse(Expression.Property(ctx, nameof(ModuleDbContext.DataScopeUnrestricted)), granted);
        return Expression.Lambda(body, entity);
    }

    private static Expression Inline(LambdaExpression lambda, ParameterExpression entity, Expression currentUser) =>
        new Rewriter(lambda.Parameters[0], entity, currentUser).Visit(lambda.Body);

    private sealed class Rewriter(ParameterExpression from, ParameterExpression to, Expression currentUser) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;

        protected override Expression VisitMember(MemberExpression node) =>
            node.Member == _currentUserPlaceholder ? currentUser : base.VisitMember(node);
    }
}
