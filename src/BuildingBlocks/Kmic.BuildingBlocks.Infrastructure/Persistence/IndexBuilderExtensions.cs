using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 索引的框架映射扩展
/// </summary>
public static class IndexBuilderExtensions
{
    /// <summary>
    /// 违反这个唯一约束时返回的模块错误，如“编号已存在”；业务表的唯一索引都必须声明，否则模型校验失败
    /// </summary>
    /// <exception cref="InvalidOperationException">索引不是唯一索引</exception>
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
