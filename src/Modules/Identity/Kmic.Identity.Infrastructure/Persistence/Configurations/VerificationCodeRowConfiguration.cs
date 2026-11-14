using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class VerificationCodeRowConfiguration : IEntityTypeConfiguration<VerificationCodeRow>
{
    public void Configure(EntityTypeBuilder<VerificationCodeRow> builder)
    {
        builder.ToTable("VerificationCode", t => t.HasComment("预留的短信验证码记录（Identity）：保存验证码哈希及校验状态；发送、校验和清理流程尚未接入"));
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
