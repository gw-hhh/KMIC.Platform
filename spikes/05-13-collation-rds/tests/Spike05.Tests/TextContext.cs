using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Spike05.Tests;

public sealed class TextContext(string connectionString, string schema) : DbContext
{
    public DbSet<TextItem> Items => Set<TextItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new KmicFunctionsExtension());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema);

        // .IsCaseInsensitive() 映射到 public."CaseInsensitive"：由迁移程序在模块迁移之前创建一次（原型 #5 D1），
        // 模型中不声明 HasCollation，否则每个模块的迁移都会尝试创建它
        modelBuilder.Entity<TextItem>(item =>
        {
            item.ToTable("TextItem");
            item.Property(i => i.Code).UseCollation("CaseInsensitive");
        });
    }
}
