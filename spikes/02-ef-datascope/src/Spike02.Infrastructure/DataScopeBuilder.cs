using System.Linq.Expressions;

namespace Spike02.Infrastructure;

/// <summary>
/// HasDataScope 之后的可选声明（05-05 第 5.8 节）
/// </summary>
public sealed class DataScopeBuilder<TEntity>
    where TEntity : class
{
    private readonly DataScopeDefinition _definition;

    internal DataScopeBuilder(DataScopeDefinition definition) => _definition = definition;

    public DataScopeBuilder<TEntity> VisibleThroughRelation(Expression<Func<TEntity, bool>> relation)
    {
        _definition.Relation = relation;
        return this;
    }

    public DataScopeBuilder<TEntity> RestrictVisibility(Expression<Func<TEntity, bool>> visibility)
    {
        _definition.Visibility = visibility;
        return this;
    }
}
