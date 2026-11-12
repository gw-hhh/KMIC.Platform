using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kmic.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable(t => t.HasComment("刷新令牌；Identity 模块；一行代表签发过的一个刷新令牌（只存哈希），随会话一起保存"));
        builder.Property(t => t.TokenHash).HasMaxLength(64).Classified(KmicData.Secret);
        builder.HasIndex(t => t.TokenHash).IsUnique().MapsToError(IdentityErrors.InvalidRefreshToken);
    }
}
