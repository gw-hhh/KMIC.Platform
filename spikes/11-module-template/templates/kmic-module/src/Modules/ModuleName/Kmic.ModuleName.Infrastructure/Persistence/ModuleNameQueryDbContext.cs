using Kmic.ModuleName.Application;
using Kmic.ModuleName.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kmic.ModuleName.Infrastructure.Persistence;

/// <summary>
/// ModuleName 模块查询用的只读上下文，不生成迁移
/// </summary>
internal sealed class ModuleNameQueryDbContext(DbContextOptions<ModuleNameQueryDbContext> options) : DbContext(options), IModuleNameQueryDb
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.HasDefaultSchema(ModuleNameModuleInfo.Slug);
}
