using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(t => t.HasComment("用户；Identity 模块；一行代表一个能登录系统的人或代表系统的固定账号；不删除只禁用"));
        builder.Property(u => u.Account).HasMaxLength(100).IsCaseInsensitive();
        builder.HasIndex(u => u.Account).IsUnique().MapsToError(IdentityErrors.AccountExists);
        builder.Property(u => u.DisplayName).HasMaxLength(100);
        builder.Property(u => u.EnglishName).HasMaxLength(100);
        builder.Property(u => u.EmployeeNo).HasMaxLength(50).IsCaseInsensitive();
        builder.HasIndex(u => u.EmployeeNo).IsUnique().HasFilter("\"EmployeeNo\" IS NOT NULL").MapsToError(IdentityErrors.EmployeeNoExists);
        builder.Property(u => u.Mobile).HasMaxLength(20).Classified(KmicData.Personal);
        builder.HasIndex(u => u.Mobile).IsUnique().HasFilter("\"Mobile\" IS NOT NULL").MapsToError(IdentityErrors.MobileExists);
        builder.Property(u => u.Email).HasMaxLength(200).Classified(KmicData.Personal);
        builder.Property(u => u.PasswordHash).HasMaxLength(200).Classified(KmicData.Secret);

        // 密码修改要求在审计中隐藏值，密码修改时间不参与审计，避免暴露密码管理信息
        builder.Property(u => u.PasswordChangeRequired).AuditHidden();
        builder.Property(u => u.PasswordChangedAt).ExcludeFromAudit();
        builder.HasIndex(u => u.OrgUnitId);
        builder.HasMany(u => u.Grants).WithOne().HasForeignKey("UserId");
        builder.HasNoDataScope("身份与授权数据按权限点管理，不按归属过滤");
    }
}
