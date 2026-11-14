using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class LoginThrottleRowConfiguration : IEntityTypeConfiguration<LoginThrottleRow>
{
    public void Configure(EntityTypeBuilder<LoginThrottleRow> builder)
    {
        builder.ToTable("LoginThrottle", t => t.HasComment("登录失败计数（Identity）：每个登录名哈希一条失败状态记录；按配置的重置间隔清理，等待或锁定未结束时保留"));
        builder.HasKey(r => r.LoginNameHash);
        builder.Property(r => r.LoginNameHash).HasMaxLength(64);
    }
}
