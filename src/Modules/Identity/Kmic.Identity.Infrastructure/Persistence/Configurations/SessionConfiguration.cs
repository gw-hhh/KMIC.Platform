using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable(t => t.HasComment("会话（Identity）：一条记录对应一次普通或模拟登录；按绝对到期或作废时间及配置保留期清理"));
        builder.Property(s => s.DeviceName).HasMaxLength(200);
        builder.Property(s => s.LoginIp).HasMaxLength(45);
        builder.Property(s => s.ImpersonationReason).HasMaxLength(500);
        builder.HasIndex(s => new { s.UserId, s.Status });
        builder.HasIndex(s => s.ImpersonatorUserId).HasFilter("\"ImpersonatorUserId\" IS NOT NULL");
        builder.HasMany(s => s.RefreshTokens).WithOne().HasForeignKey("SessionId");
        builder.HasNoDataScope("会话按用户与令牌访问，不按归属过滤");
    }
}
