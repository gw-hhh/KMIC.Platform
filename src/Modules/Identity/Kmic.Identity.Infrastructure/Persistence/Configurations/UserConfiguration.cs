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

        // 名字里有 Password 的另外两列不是密钥，但审计里不记它们的值：“是否必须改密码”“密码何时变化”不该暴露给查看审计的人
        builder.Property(u => u.PasswordChangeRequired).AuditHidden();
        builder.Property(u => u.PasswordChangedAt).ExcludeFromAudit();
        builder.HasIndex(u => u.OrgUnitId);
        builder.HasMany(u => u.Grants).WithOne().HasForeignKey("UserId");
        builder.HasNoDataScope("身份与授权数据按权限点管理，不按归属过滤");
    }
}
