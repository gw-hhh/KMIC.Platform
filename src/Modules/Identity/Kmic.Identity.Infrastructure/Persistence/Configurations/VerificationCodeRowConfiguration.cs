using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class VerificationCodeRowConfiguration : IEntityTypeConfiguration<VerificationCodeRow>
{
    public void Configure(EntityTypeBuilder<VerificationCodeRow> builder)
    {
        builder.ToTable("VerificationCode", t => t.HasComment("短信验证码；Identity 模块；一行代表一次发送请求（哈希），过期超过 1 天由每日清理任务删除"));
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.PhoneHash).HasMaxLength(64);
        builder.Property(r => r.Purpose).HasMaxLength(20);
        builder.Property(r => r.CodeHash).HasMaxLength(64);
        builder.Property(r => r.Status).HasMaxLength(16);
        builder.Property(r => r.RequestIp).HasMaxLength(45);
        builder.HasIndex(r => new { r.PhoneHash, r.CreatedAt });
        builder.HasIndex(r => new { r.RequestIp, r.CreatedAt });
    }
}
