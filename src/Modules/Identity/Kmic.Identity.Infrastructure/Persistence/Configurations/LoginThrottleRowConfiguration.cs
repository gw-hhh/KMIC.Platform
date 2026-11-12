using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class LoginThrottleRowConfiguration : IEntityTypeConfiguration<LoginThrottleRow>
{
    public void Configure(EntityTypeBuilder<LoginThrottleRow> builder)
    {
        builder.ToTable("LoginThrottle", t => t.HasComment("登录失败计数；Identity 模块；一行代表一个登录名（哈希）的失败状态；24 小时没有新失败由每日清理任务删除"));
        builder.HasKey(r => r.LoginNameHash);
        builder.Property(r => r.LoginNameHash).HasMaxLength(64);
    }
}
