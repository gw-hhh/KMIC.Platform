using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Preferences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Configuration.Infrastructure.Persistence.Configurations;

internal sealed class PreferenceValueConfiguration : IEntityTypeConfiguration<PreferenceValue>
{
    public void Configure(EntityTypeBuilder<PreferenceValue> builder)
    {
        builder.ToTable(t =>
        {
            t.HasComment("分层偏好值（Configuration）：按键、层级、角色和用户组合唯一；系统层无归属，角色层按角色保存，个人层按用户和角色保存");

            // 用数据库检查约束保证层级与角色、用户归属一致，手工写入也须满足
            t.HasCheckConstraint(
                "CK_PreferenceValue_LayerOwner",
                """
                ("Layer" = 'System' AND "RoleId" IS NULL AND "UserId" IS NULL)
                OR ("Layer" = 'Role' AND "RoleId" IS NOT NULL AND "UserId" IS NULL)
                OR ("Layer" = 'UserRole' AND "RoleId" IS NOT NULL AND "UserId" IS NOT NULL)
                """);
        });
        builder.Property(v => v.Key).HasMaxLength(PreferenceValue.MaxKeyLength);
        builder.Property(v => v.Value).HasColumnType("jsonb");

        // 空归属组合仍须唯一；普通唯一索引将 null 视为不同值，因此显式关闭空值区分
        builder.HasIndex(v => new { v.Key, v.Layer, v.RoleId, v.UserId })
            .IsUnique()
            .AreNullsDistinct(false)
            .MapsToError(CommonErrors.VersionConflict);

        // 读取一个角色或一个用户的全部偏好
        builder.HasIndex(v => new { v.Layer, v.RoleId, v.UserId });
        builder.HasNoDataScope("偏好的用户层按用户 ID 显式读取，不是数据范围；系统层与角色层对所有登录用户一致，按权限点管理");
    }
}
