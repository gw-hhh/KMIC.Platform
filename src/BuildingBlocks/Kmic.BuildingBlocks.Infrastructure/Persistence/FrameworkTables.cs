using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 框架表
/// </summary>
/// <remarks>
/// 每个模块的 schema 里都有，和业务数据在同一个事务里写入
/// </remarks>
internal static class FrameworkTables
{
    public const string Idempotency = nameof(IdempotencyRecord);

    public static void Apply(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<IdempotencyRecord>(record =>
        {
            record.ToTable(Idempotency, t => t.HasComment("幂等记录"));
            record.Property(r => r.Id).ValueGeneratedNever();
            record.Property(r => r.Operation).HasMaxLength(200);
            record.Property(r => r.Key).HasMaxLength(100);
            record.Property(r => r.Status).HasMaxLength(32);
            record.Property(r => r.ResponseBody).HasColumnType(ColumnTypes.Json);
            record.HasIndex(r => new { r.UserId, r.RoleId, r.Operation, r.Key }).IsUnique();
            record.HasIndex(r => r.ExpiresAt);
        });
}
