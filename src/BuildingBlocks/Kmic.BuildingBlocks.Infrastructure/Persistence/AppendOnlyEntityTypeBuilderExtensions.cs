using Kmic.BuildingBlocks.Infrastructure.History;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class AppendOnlyEntityTypeBuilderExtensions
{
    /// <summary>声明为只追加表（运行账号仅允许插入与查询，过期数据由迁移工具清理）</summary>
    public static void IsAppendOnly<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasAnnotation(StateHistory.AppendOnlyAnnotation, true);
    }
}
