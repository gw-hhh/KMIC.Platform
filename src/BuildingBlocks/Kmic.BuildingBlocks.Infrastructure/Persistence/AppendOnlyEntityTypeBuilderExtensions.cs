using Kmic.BuildingBlocks.Infrastructure.History;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 只追加的表
/// </summary>
public static class AppendOnlyEntityTypeBuilderExtensions
{
    /// <summary>
    /// 声明这张表只追加，运行账号只能插入与查询
    /// </summary>
    /// <remarks>
    /// 过期数据由迁移程序的维护命令删除
    /// </remarks>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public static void IsAppendOnly<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasAnnotation(StateHistory.AppendOnlyAnnotation, true);
    }
}
