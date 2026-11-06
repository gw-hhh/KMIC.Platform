using Kmic.ModuleName.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.ModuleName.Infrastructure.Persistence;

/// <summary>
/// ModuleName 模块写入用的上下文，迁移也由它生成
/// </summary>
internal sealed class ModuleNameDbContext(DbContextOptions<ModuleNameDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.HasDefaultSchema(ModuleNameModuleInfo.Slug);
}
