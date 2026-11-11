using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 索引映射扩展
/// </summary>
public static class IndexBuilderExtensions
{
    /// <summary>
    /// 重复时返回的错误
    /// </summary>
    /// <remarks>
    /// 业务表的唯一索引都要指定，否则模型校验不通过
    /// </remarks>
    /// <exception cref="InvalidOperationException">不是唯一索引</exception>
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
