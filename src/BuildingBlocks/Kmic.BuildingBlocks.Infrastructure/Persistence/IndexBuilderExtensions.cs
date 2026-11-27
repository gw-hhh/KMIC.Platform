using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class IndexBuilderExtensions
{
    /// <summary>声明唯一索引冲突时映射的领域错误（业务表所有唯一索引均须指定）</summary>
    public static IndexBuilder<TEntity> MapsToError<TEntity>(this IndexBuilder<TEntity> builder, Error error)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(error);
        if (builder.Metadata.IsUnique is not true)
        {
            throw new InvalidOperationException("MapsToError 只能用于唯一索引，先调用 IsUnique()。");
        }

        builder.Metadata.SetAnnotation(ModelAnnotations.UniqueError, error);
        return builder;
    }
}
