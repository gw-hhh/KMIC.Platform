using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 每个模块 schema 中都有的框架表；它们与业务数据同一事务写入，不加审计列、不用强类型 ID
/// </summary>
internal static class FrameworkTables
{
    public const string Idempotency = nameof(IdempotencyRecord);

    public static void Apply(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<IdempotencyRecord>(record =>
        {
            record.ToTable(Idempotency, t => t.HasComment("幂等记录；框架表；一行代表一次带幂等键的命令；同一用户、角色、操作、键唯一；过期后由清理任务删除"));
            record.Property(r => r.Id).ValueGeneratedNever();
            record.Property(r => r.Operation).HasMaxLength(200);
            record.Property(r => r.Key).HasMaxLength(100);
            record.Property(r => r.Status).HasMaxLength(32);
            record.Property(r => r.ResponseBody).HasColumnType(ColumnTypes.Json);
            record.HasIndex(r => new { r.UserId, r.RoleId, r.Operation, r.Key }).IsUnique();
            record.HasIndex(r => r.ExpiresAt);
        });
}
