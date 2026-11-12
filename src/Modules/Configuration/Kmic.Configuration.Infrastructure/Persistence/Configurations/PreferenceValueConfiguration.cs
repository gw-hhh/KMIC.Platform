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
            t.HasComment("偏好在某一层上的值；Configuration 模块；系统层、角色层、用户在角色下的层，各层同一个键只有一行");

            // 层与两个归属列一一对应，工厂方法之外的写入（手工修数据）也不能破坏它
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

        // 归属列为空的行也要互相排斥：系统层的两列都是空，普通唯一索引会把空值当成互不相同
        builder.HasIndex(v => new { v.Key, v.Layer, v.RoleId, v.UserId })
            .IsUnique()
            .AreNullsDistinct(false)
            .MapsToError(CommonErrors.VersionConflict);

        // 读取一个角色或一个用户的全部偏好
        builder.HasIndex(v => new { v.Layer, v.RoleId, v.UserId });
        builder.HasNoDataScope("偏好的用户层按用户 ID 显式读取，不是数据范围；系统层与角色层对所有登录用户一致，按权限点管理");
    }
}
