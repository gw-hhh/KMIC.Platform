using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Spike15.Tests;

// 同一个上下文类型，按传入的数据源分别充当写入上下文（事务池）与查询上下文（读池）
public sealed class ItemsContext(NpgsqlDataSource dataSource, bool readOnly, QueryRetry retry = QueryRetry.None) : DbContext
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // 05-02 D4：查询启用 EF 的重试执行策略，命令不启用
        optionsBuilder.UseNpgsql(dataSource, o =>
        {
            switch (retry)
            {
                case QueryRetry.EfDefault:
                    o.EnableRetryOnFailure();
                    break;
                case QueryRetry.Kmic:
                    o.ExecutionStrategy(d => new QueryRetryStrategy(d));
                    break;
            }
        });
        if (readOnly)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Item>().ToTable("Item");
}
