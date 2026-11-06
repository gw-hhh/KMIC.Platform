using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spike02.Domain;

namespace Spike02.Infrastructure;

/// <summary>
/// 在实体配置中声明数据范围（05-05 第 5.4 节）
/// </summary>
public static class DataScopeEntityTypeBuilderExtensions
{
    public static DataScopeBuilder<TEntity> HasDataScope<TEntity>(this EntityTypeBuilder<TEntity> builder, string scopeObject)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (!typeof(IHasOwner).IsAssignableFrom(typeof(TEntity)) && !typeof(IHasOrgUnit).IsAssignableFrom(typeof(TEntity)))
        {
            throw new InvalidOperationException($"{typeof(TEntity).Name} must implement IHasOwner or IHasOrgUnit to have a data scope.");
        }

        var definition = new DataScopeDefinition(scopeObject);
        builder.Metadata.SetAnnotation(DataScopeDefinition.AnnotationName, definition);
        return new DataScopeBuilder<TEntity>(definition);
    }
}
