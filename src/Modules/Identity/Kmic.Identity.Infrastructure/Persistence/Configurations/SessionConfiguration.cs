using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable(t => t.HasComment("会话；Identity 模块；一行代表一次登录（含模拟身份）；作废或过期 30 天后由清理任务删除"));
        builder.Property(s => s.DeviceName).HasMaxLength(200);
        builder.Property(s => s.LoginIp).HasMaxLength(45);
        builder.Property(s => s.ImpersonationReason).HasMaxLength(500);
        builder.HasIndex(s => new { s.UserId, s.Status });
        builder.HasIndex(s => s.ImpersonatorUserId).HasFilter("\"ImpersonatorUserId\" IS NOT NULL");
        builder.HasMany(s => s.RefreshTokens).WithOne().HasForeignKey("SessionId");
        builder.HasNoDataScope("会话按用户与令牌访问，不按归属过滤");
    }
}
