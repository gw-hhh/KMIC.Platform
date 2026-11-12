using Kmic.BuildingBlocks.Infrastructure.History;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 只追加的表
/// </summary>
public static class AppendOnlyEntityTypeBuilderExtensions
{
    /// <summary>
    /// 声明这张表只追加：建表迁移自动对运行账号撤销修改和删除权限，应用只能插入与查询
    /// </summary>
    /// <remarks>
    /// 用在“事后不能改”的记录上，如设置的变更历史。过期数据由迁移程序的维护命令删除，不放在应用的定时任务里
    /// </remarks>
    /// <param name="builder">实体的配置</param>
    /// <typeparam name="TEntity">实体类型</typeparam>
    public static void IsAppendOnly<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasAnnotation(StateHistory.AppendOnlyAnnotation, true);
    }
}
